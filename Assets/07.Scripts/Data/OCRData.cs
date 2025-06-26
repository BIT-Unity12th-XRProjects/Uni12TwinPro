using UnityEngine;

namespace Test.Scripts.Data
{
    public class OCRDataModel
    {
        public string ocrInvokeUrl;
        public string ocrSecretKey;
    }
    
    public class OCRData
    {
        public OCRDataModel OcrDataModel { get; private set; }

        public OCRData()
        {
            Initialize();
        }

        private void Initialize()
        {
            TextAsset json =  Resources.Load<TextAsset>("Json/OCRAPI");
            
            OcrDataModel =  JsonUtility.FromJson<OCRDataModel>(json.text);
        }
    }
}