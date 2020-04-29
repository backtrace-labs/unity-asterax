﻿using Backtrace.Unity.Model;
using Backtrace.Unity.Model.JsonData;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine.TestTools;

namespace Tests
{
    public class SerializationTests
    {
        [UnityTest]
        public IEnumerator TestDataSerialization_ValidReport_DataSerializeAndDeserialize()
        {
            var report = new BacktraceReport(new Exception("test"));
            var data = new BacktraceData(report, null);
            var json = data.ToJson();
            var deserializedData = BacktraceData.Deserialize(json);
            Assert.IsTrue(deserializedData.Timestamp == data.Timestamp);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestAnnotations_ValidReport_AnnotationExists()
        {
            var report = new BacktraceReport(new Exception("test"));
            var data = new BacktraceData(report, null);
            var json = data.Annotation.ToJson();
            var deserializedData = Annotations.Deserialize(json);
            Assert.IsTrue(deserializedData.EnvironmentVariables.Count == data.Annotation.EnvironmentVariables.Count);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestAttributes_ValidReport_AttributesExists()
        {
            var report = new BacktraceReport(new Exception("test"));
            var data = new BacktraceData(report, null);
            var json = data.Attributes.ToJson();
            var deserializedData = BacktraceAttributes.Deserialize(json);
            Assert.IsTrue(deserializedData.Attributes.Count == data.Attributes.Attributes.Count);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestReportSerialization_ValidReport_ReportsAreTheSame()
        {
            var report = new BacktraceReport(new Exception("test"));
            var json = report.ToJson();
            var deserializedReport = BacktraceReport.Deserialize(json);
            Assert.IsTrue(deserializedReport.Uuid == report.Uuid);
            Assert.IsTrue(deserializedReport.Classifier == report.Classifier);
            Assert.IsTrue(deserializedReport.Message == report.Message);
            Assert.IsTrue(deserializedReport.Timestamp == report.Timestamp);
            Assert.IsTrue(deserializedReport.DiagnosticStack.Count == report.DiagnosticStack.Count);
            Assert.IsTrue(deserializedReport.Fingerprint == report.Fingerprint);
            Assert.IsTrue(deserializedReport.Factor == report.Factor);
            Assert.IsTrue(deserializedReport.ExceptionTypeReport == report.ExceptionTypeReport);
            Assert.IsTrue(deserializedReport.Attributes.Count == report.Attributes.Count);
            Assert.IsTrue(deserializedReport.AttachmentPaths.Count == report.AttachmentPaths.Count);
            yield return null;
        }
    }
}
