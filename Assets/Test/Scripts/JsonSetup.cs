using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

static class JsonSetup
{
    // 스트리퍼가 보고 ‘사용 중’이라 판단하도록 더미 인스턴스 확보
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Configure()
    {
        JsonConverter _keep = new TwinJsonConverter();

        X509Thumbprint _keep2 = new X509Thumbprint();

        TwinCollection _keep3 = new TwinCollection();
    }
}

