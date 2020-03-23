using System.Collections.Generic;
using Backtrace.Newtonsoft.Shims;

namespace Backtrace.Newtonsoft.Linq.JsonPath
{
    [Preserve]
    internal class ScanFilter : PathFilter
    {
        public string Name { get; set; }

        public override IEnumerable<JToken> ExecuteFilter(IEnumerable<JToken> current, bool errorWhenNoMatch)
        {
            foreach (JToken root in current)
            {
                if (Name == null)
                {
                    yield return root;
                }

                JToken value = root;
                JToken container = root;

                while (true)
                {
                    if (container != null && container.HasValues)
                    {
                        value = container.First;
                    }
                    else
                    {
                        while (value != null && value != root && value == value.Parent.Last)
                        {
                            value = value.Parent;
                        }

                        if (value == null || value == root)
                        {
                            break;
                        }

                        value = value.Next;
                    }

                    BacktraceJProperty e = value as BacktraceJProperty;
                    if (e != null)
                    {
                        if (e.Name == Name)
                        {
                            yield return e.Value;
                        }
                    }
                    else
                    {
                        if (Name == null)
                        {
                            yield return value;
                        }
                    }

                    container = value as BacktraceJContainer;
                }
            }
        }
    }
}