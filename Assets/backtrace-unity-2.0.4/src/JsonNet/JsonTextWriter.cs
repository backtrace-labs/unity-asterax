﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using Backtrace.Newtonsoft.Shims;
using Backtrace.Newtonsoft.Utilities;
using System;
using System.Globalization;
using System.IO;

namespace Backtrace.Newtonsoft
{
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
    /// </summary>
    [Preserve]
    public class JsonTextWriter : JsonWriter
    {
        private readonly TextWriter _writer;
        private Base64Encoder _base64Encoder;
        private char _indentChar;
        private int _indentation;
        private char _quoteChar;
        private bool _quoteName;
        private bool[] _charEscapeFlags;
        private char[] _writeBuffer;
        private IArrayPool<char> _arrayPool;
        private char[] _indentChars;

        private Base64Encoder Base64Encoder
        {
            get
            {
                if (_base64Encoder == null)
                {
                    _base64Encoder = new Base64Encoder(_writer);
                }

                return _base64Encoder;
            }
        }

        /// <summary>
        /// Gets or sets the writer's character array pool.
        /// </summary>
        public IArrayPool<char> ArrayPool
        {
            get { return _arrayPool; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _arrayPool = value;
            }
        }

        /// <summary>
        /// Gets or sets how many IndentChars to write for each level in the hierarchy when <see cref="Formatting"/> is set to <c>Formatting.Indented</c>.
        /// </summary>
        public int Indentation
        {
            get { return _indentation; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Indentation value must be greater than 0.");
                }

                _indentation = value;
            }
        }

        /// <summary>
        /// Gets or sets which character to use to quote attribute values.
        /// </summary>
        public char QuoteChar
        {
            get { return _quoteChar; }
            set
            {
                if (value != '"' && value != '\'')
                {
                    throw new ArgumentException(@"Invalid JavaScript string quote character. Valid quote characters are ' and "".");
                }

                _quoteChar = value;
                UpdateCharEscapeFlags();
            }
        }

        /// <summary>
        /// Gets or sets which character to use for indenting when <see cref="Formatting"/> is set to <c>Formatting.Indented</c>.
        /// </summary>
        public char IndentChar
        {
            get { return _indentChar; }
            set
            {
                if (value != _indentChar)
                {
                    _indentChar = value;
                    _indentChars = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether object names will be surrounded with quotes.
        /// </summary>
        public bool QuoteName
        {
            get { return _quoteName; }
            set { _quoteName = value; }
        }

        /// <summary>
        /// Creates an instance of the <c>JsonWriter</c> class using the specified <see cref="TextWriter"/>. 
        /// </summary>
        /// <param name="textWriter">The <c>TextWriter</c> to write to.</param>
        public JsonTextWriter(TextWriter textWriter)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }

            _writer = textWriter;
            _quoteChar = '"';
            _quoteName = true;
            _indentChar = ' ';
            _indentation = 2;

            UpdateCharEscapeFlags();
        }

        /// <summary>
        /// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            _writer.Flush();
        }

        /// <summary>
        /// Closes this stream and the underlying stream.
        /// </summary>
        public override void Close()
        {
            base.Close();

            if (_writeBuffer != null)
            {
                BufferUtils.ReturnBuffer(_arrayPool, _writeBuffer);
                _writeBuffer = null;
            }

            if (CloseOutput && _writer != null)
            {
#if !(DOTNET || PORTABLE40 || PORTABLE)
                _writer.Close();
#else
                _writer.Dispose();
#endif
            }
        }

        /// <summary>
        /// Writes the beginning of a JSON object.
        /// </summary>
        public override void WriteStartObject()
        {
            InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);

            _writer.Write('{');
        }

        /// <summary>
        /// Writes the beginning of a JSON array.
        /// </summary>
        public override void WriteStartArray()
        {
            InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);

            _writer.Write('[');
        }

        /// <summary>
        /// Writes the start of a constructor with the given name.
        /// </summary>
        /// <param name="name">The name of the constructor.</param>
        public override void WriteStartConstructor(string name)
        {
            InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);

            _writer.Write("new ");
            _writer.Write(name);
            _writer.Write('(');
        }

        /// <summary>
        /// Writes the specified end token.
        /// </summary>
        /// <param name="token">The end token to write.</param>
        protected override void WriteEnd(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.EndObject:
                    _writer.Write('}');
                    break;
                case JsonToken.EndArray:
                    _writer.Write(']');
                    break;
                case JsonToken.EndConstructor:
                    _writer.Write(')');
                    break;
                default:
                    throw JsonWriterException.Create(this, "Invalid JsonToken: " + token, null);
            }
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public override void WritePropertyName(string name)
        {
            InternalWritePropertyName(name);

            WriteEscapedString(name, _quoteName);

            _writer.Write(':');
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
        public override void WritePropertyName(string name, bool escape)
        {
            InternalWritePropertyName(name);

            if (escape)
            {
                WriteEscapedString(name, _quoteName);
            }
            else
            {
                if (_quoteName)
                {
                    _writer.Write(_quoteChar);
                }

                _writer.Write(name);

                if (_quoteName)
                {
                    _writer.Write(_quoteChar);
                }
            }

            _writer.Write(':');
        }

        internal override void OnStringEscapeHandlingChanged()
        {
            UpdateCharEscapeFlags();
        }

        private void UpdateCharEscapeFlags()
        {
            _charEscapeFlags = JavaScriptUtils.GetCharEscapeFlags(StringEscapeHandling, _quoteChar);
        }

        /// <summary>
        /// Writes indent characters.
        /// </summary>
        protected override void WriteIndent()
        {
            _writer.WriteLine();

            // levels of indentation multiplied by the indent count
            int currentIndentCount = Top * _indentation;

            if (currentIndentCount > 0)
            {
                if (_indentChars == null)
                {
                    _indentChars = new string(_indentChar, 10).ToCharArray();
                }

                while (currentIndentCount > 0)
                {
                    int writeCount = Math.Min(currentIndentCount, 10);

                    _writer.Write(_indentChars, 0, writeCount);

                    currentIndentCount -= writeCount;
                }
            }
        }

        /// <summary>
        /// Writes the JSON value delimiter.
        /// </summary>
        protected override void WriteValueDelimiter()
        {
            _writer.Write(',');
        }

        /// <summary>
        /// Writes an indent space.
        /// </summary>
        protected override void WriteIndentSpace()
        {
            _writer.Write(' ');
        }

        private void WriteValueInternal(string value, JsonToken token)
        {
            _writer.Write(value);
        }

        #region WriteValue methods
        /// <summary>
        /// Writes a <see cref="object"/> value.
        /// An error will raised if the value cannot be written as a single JSON token.
        /// </summary>
        /// <param name="value">The <see cref="object"/> value to write.</param>
        public override void WriteValue(object value)
        {
            base.WriteValue(value);
        }

        /// <summary>
        /// Writes a null value.
        /// </summary>
        public override void WriteNull()
        {
            InternalWriteValue(JsonToken.Null);
            WriteValueInternal(BacktraceDataConverter.Null, JsonToken.Null);
        }

        /// <summary>
        /// Writes an undefined value.
        /// </summary>
        public override void WriteUndefined()
        {
            InternalWriteValue(JsonToken.Undefined);
            WriteValueInternal(BacktraceDataConverter.Undefined, JsonToken.Undefined);
        }

        /// <summary>
        /// Writes raw JSON.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public override void WriteRaw(string json)
        {
            InternalWriteRaw();

            _writer.Write(json);
        }

        /// <summary>
        /// Writes a <see cref="string"/> value.
        /// </summary>
        /// <param name="value">The <see cref="string"/> value to write.</param>
        public override void WriteValue(string value)
        {
            InternalWriteValue(JsonToken.String);

            if (value == null)
            {
                WriteValueInternal(BacktraceDataConverter.Null, JsonToken.Null);
            }
            else
            {
                WriteEscapedString(value, true);
            }
        }

        private void WriteEscapedString(string value, bool quote)
        {
            EnsureWriteBuffer();
            JavaScriptUtils.WriteEscapedJavaScriptString(_writer, value, _quoteChar, quote, _charEscapeFlags, StringEscapeHandling, _arrayPool, ref _writeBuffer);
        }

        /// <summary>
        /// Writes a <see cref="int"/> value.
        /// </summary>
        /// <param name="value">The <see cref="int"/> value to write.</param>
        public override void WriteValue(int value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="uint"/> value.
        /// </summary>
        /// <param name="value">The <see cref="uint"/> value to write.</param>
        public override void WriteValue(uint value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="long"/> value.
        /// </summary>
        /// <param name="value">The <see cref="long"/> value to write.</param>
        public override void WriteValue(long value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="ulong"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ulong"/> value to write.</param>
        public override void WriteValue(ulong value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="float"/> value.
        /// </summary>
        /// <param name="value">The <see cref="float"/> value to write.</param>
        public override void WriteValue(float value)
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(BacktraceDataConverter.ToString(value, FloatFormatHandling, QuoteChar, false), JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Single}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Single}"/> value to write.</param>
        public override void WriteValue(float? value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.Float);
                WriteValueInternal(BacktraceDataConverter.ToString(value.GetValueOrDefault(), FloatFormatHandling, QuoteChar, true), JsonToken.Float);
            }
        }

        /// <summary>
        /// Writes a <see cref="double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="double"/> value to write.</param>
        public override void WriteValue(double value)
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(BacktraceDataConverter.ToString(value, FloatFormatHandling, QuoteChar, false), JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Double}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Double}"/> value to write.</param>
        public override void WriteValue(double? value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.Float);
                WriteValueInternal(BacktraceDataConverter.ToString(value.GetValueOrDefault(), FloatFormatHandling, QuoteChar, true), JsonToken.Float);
            }
        }

        /// <summary>
        /// Writes a <see cref="bool"/> value.
        /// </summary>
        /// <param name="value">The <see cref="bool"/> value to write.</param>
        public override void WriteValue(bool value)
        {
            InternalWriteValue(JsonToken.Boolean);
            WriteValueInternal(BacktraceDataConverter.ToString(value), JsonToken.Boolean);
        }

        /// <summary>
        /// Writes a <see cref="short"/> value.
        /// </summary>
        /// <param name="value">The <see cref="short"/> value to write.</param>
        public override void WriteValue(short value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="ushort"/> value.
        /// </summary>
        /// <param name="value">The <see cref="ushort"/> value to write.</param>

        public override void WriteValue(ushort value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="char"/> value to write.</param>
        public override void WriteValue(char value)
        {
            InternalWriteValue(JsonToken.String);
            WriteValueInternal(BacktraceDataConverter.ToString(value), JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="byte"/> value to write.</param>
        public override void WriteValue(byte value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="sbyte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="sbyte"/> value to write.</param>

        public override void WriteValue(sbyte value)
        {
            InternalWriteValue(JsonToken.Integer);
            WriteIntegerValue(value);
        }

        /// <summary>
        /// Writes a <see cref="decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="decimal"/> value to write.</param>
        public override void WriteValue(decimal value)
        {
            InternalWriteValue(JsonToken.Float);
            WriteValueInternal(BacktraceDataConverter.ToString(value), JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to write.</param>
        public override void WriteValue(DateTime value)
        {
            InternalWriteValue(JsonToken.Date);
            value = DateTimeUtils.EnsureDateTime(value, DateTimeZoneHandling);

            if (string.IsNullOrEmpty(DateFormatString))
            {
                EnsureWriteBuffer();

                int pos = 0;
                _writeBuffer[pos++] = _quoteChar;
                pos = DateTimeUtils.WriteDateTimeString(_writeBuffer, pos, value, null, value.Kind, DateFormatHandling);
                _writeBuffer[pos++] = _quoteChar;

                _writer.Write(_writeBuffer, 0, pos);
            }
            else
            {
                _writer.Write(_quoteChar);
                _writer.Write(value.ToString(DateFormatString, Culture));
                _writer.Write(_quoteChar);
            }
        }

        /// <summary>
        /// Writes a <see cref="byte"/>[] value.
        /// </summary>
        /// <param name="value">The <see cref="byte"/>[] value to write.</param>
        public override void WriteValue(byte[] value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.Bytes);
                _writer.Write(_quoteChar);
                Base64Encoder.Encode(value, 0, value.Length);
                Base64Encoder.Flush();
                _writer.Write(_quoteChar);
            }
        }

#if !NET20
        /// <summary>
        /// Writes a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
        public override void WriteValue(DateTimeOffset value)
        {
            InternalWriteValue(JsonToken.Date);

            if (string.IsNullOrEmpty(DateFormatString))
            {
                EnsureWriteBuffer();

                int pos = 0;
                _writeBuffer[pos++] = _quoteChar;
                pos = DateTimeUtils.WriteDateTimeString(_writeBuffer, pos, (DateFormatHandling == DateFormatHandling.IsoDateFormat) ? value.DateTime : value.UtcDateTime, value.Offset, DateTimeKind.Local, DateFormatHandling);
                _writeBuffer[pos++] = _quoteChar;

                _writer.Write(_writeBuffer, 0, pos);
            }
            else
            {
                _writer.Write(_quoteChar);
                _writer.Write(value.ToString(DateFormatString, Culture));
                _writer.Write(_quoteChar);
            }
        }
#endif

        /// <summary>
        /// Writes a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> value to write.</param>
        public override void WriteValue(Guid value)
        {
            InternalWriteValue(JsonToken.String);

            string text = null;

#if !(DOTNET || PORTABLE40 || PORTABLE)
            text = value.ToString("D", CultureInfo.InvariantCulture);
#else
            text = value.ToString("D");
#endif

            _writer.Write(_quoteChar);
            _writer.Write(text);
            _writer.Write(_quoteChar);
        }

        /// <summary>
        /// Writes a <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
        public override void WriteValue(TimeSpan value)
        {
            InternalWriteValue(JsonToken.String);

            string text;
#if (NET35 || NET20)
            text = value.ToString();
#else
            text = value.ToString(null, CultureInfo.InvariantCulture);
#endif

            _writer.Write(_quoteChar);
            _writer.Write(text);
            _writer.Write(_quoteChar);
        }

        /// <summary>
        /// Writes a <see cref="Uri"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Uri"/> value to write.</param>
        public override void WriteValue(Uri value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                InternalWriteValue(JsonToken.String);
                WriteEscapedString(value.OriginalString, true);
            }
        }
        #endregion

        /// <summary>
        /// Writes out a comment <code>/*...*/</code> containing the specified text. 
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public override void WriteComment(string text)
        {
            InternalWriteComment();

            _writer.Write("/*");
            _writer.Write(text);
            _writer.Write("*/");
        }

        /// <summary>
        /// Writes out the given white space.
        /// </summary>
        /// <param name="ws">The string of white space characters.</param>
        public override void WriteWhitespace(string ws)
        {
            InternalWriteWhitespace(ws);

            _writer.Write(ws);
        }

        private void EnsureWriteBuffer()
        {
            if (_writeBuffer == null)
            {
                // maximum buffer sized used when writing iso date
                _writeBuffer = BufferUtils.RentBuffer(_arrayPool, 35);
            }
        }

        private void WriteIntegerValue(long value)
        {
            if (value >= 0 && value <= 9)
            {
                _writer.Write((char)('0' + value));
            }
            else
            {
                ulong uvalue = (value < 0) ? (ulong)-value : (ulong)value;

                if (value < 0)
                {
                    _writer.Write('-');
                }

                WriteIntegerValue(uvalue);
            }
        }

        private void WriteIntegerValue(ulong uvalue)
        {
            if (uvalue <= 9)
            {
                _writer.Write((char)('0' + uvalue));
            }
            else
            {
                EnsureWriteBuffer();

                int totalLength = MathUtils.IntLength(uvalue);
                int length = 0;

                do
                {
                    _writeBuffer[totalLength - ++length] = (char)('0' + (uvalue % 10));
                    uvalue /= 10;
                } while (uvalue != 0);

                _writer.Write(_writeBuffer, 0, length);
            }
        }
    }
}