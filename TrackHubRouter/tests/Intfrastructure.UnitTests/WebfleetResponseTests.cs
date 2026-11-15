// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Text.Json;
using TrackHub.Router.Infrastructure.Webfleet.Helpers;
using TrackHub.Router.Infrastructure.Webfleet.Models;

namespace TrackHub.Router.Infrastructure.Webfleet.Tests;

[TestFixture]
public class WebfleetResponseTests
{
    private static JsonElement Json(string json) => JsonDocument.Parse(json).RootElement.Clone();

    [Test]
    public void EnsureSuccess_WithEmptyArray_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => WebfleetResponse.EnsureSuccess(Json("[]"), "showTracks"));
        Assert.That(WebfleetResponse.ReadList<TrackPoint>(Json("[]")), Is.Empty);
    }

    [TestCase("{\"errorCode\":8011,\"errorMsg\":\"request quota reached\"}", 8011)]
    [TestCase("{\"errorCode\":\"8015\",\"errorMsg\":\"busy\"}", 8015)]
    [TestCase("{\"errorcode\":1106}", 1106)]
    public void EnsureSuccess_WithErrorObject_ThrowsWithCode(string body, int code)
    {
        var exception = Assert.Throws<WebfleetApiException>(() => WebfleetResponse.EnsureSuccess(Json(body), "showTracks"));

        Assert.That(exception!.Code, Is.EqualTo(code));
    }

    [Test]
    public void EnsureSuccess_WithObjectWithoutCode_Throws()
    {
        var exception = Assert.Throws<WebfleetApiException>(() => WebfleetResponse.EnsureSuccess(Json("{\"unexpected\":true}"), "showTracks"));

        Assert.That(exception!.Code, Is.Null);
    }

    [TestCase("null")]
    [TestCase("\"63,document is empty\"")]
    public void EnsureSuccess_WithNonArrayNonObject_Throws(string body)
        => Assert.Throws<WebfleetApiException>(() => WebfleetResponse.EnsureSuccess(Json(body), "showTracks"));

    [Test]
    public void EnsureSuccess_WithUndefinedBody_Throws()
        => Assert.Throws<WebfleetApiException>(() => WebfleetResponse.EnsureSuccess(default, "showTracks"));

    [Test]
    public void NoDataError_IsAnEmptyResult()
    {
        var body = Json("{\"errorCode\":63,\"errorMsg\":\"document is empty\"}");

        Assert.DoesNotThrow(() => WebfleetResponse.EnsureSuccess(body, "showTracks"));
        Assert.That(WebfleetResponse.ReadList<TrackPoint>(body), Is.Empty);
    }

    [Test]
    public void ReadList_ReadsDocumentedColumnsIncludingNumbersSentAsStrings()
    {
        var body = Json("""
            [{"objectno":"042","objectname":"Van 42","deleted":false,"pos_time":"2026-07-17T09:20:00Z",
              "latitude_mdeg":51339695,"longitude_mdeg":12373074,"latitude":"51°20'22.9\" N",
              "speed":"63","course":210,"postext":"Leipzig","ignition":1,"odometer":"1234",
              "odometer_long":123456,"engine_operation_time":7200}]
            """);

        var report = WebfleetResponse.ReadList<ObjectReport>(body).Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(report.Objectno, Is.EqualTo("042"));
            Assert.That(report.Pos_time, Is.EqualTo(new DateTimeOffset(2026, 7, 17, 9, 20, 0, TimeSpan.Zero)));
            Assert.That(report.Latitude_mdeg, Is.EqualTo(51339695));
            Assert.That(report.Speed, Is.EqualTo(63));
            Assert.That(report.Odometer, Is.EqualTo(1234));
            Assert.That(report.Odometer_long, Is.EqualTo(123456));
            Assert.That(report.Engine_operation_time, Is.EqualTo(7200));
        }
    }

    [TestCase("true", true)]
    [TestCase("1", true)]
    [TestCase("\"1\"", true)]
    [TestCase("\"false\"", false)]
    [TestCase("0", false)]
    public void ReadList_ReadsTheDeletedFlagInAnyForm(string json, bool expected)
    {
        var report = WebfleetResponse.ReadList<ObjectReport>(Json($$"""[{"objectno":"042","deleted":{{json}}}]""")).Single();

        Assert.That(report.Deleted, Is.EqualTo(expected));
    }
}
