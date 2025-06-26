using UnityEngine;

namespace Test.Scripts.Data
{
    public class AzureDataModel
    {
        public string connectionString;
        public string deviceId;
    }
    
    public class AzureData
    {
        public AzureDataModel azureDataModel { get; private set; }
        
        public AzureData()
        {
            Initialize();
        }

        public void Initialize()
        {
            TextAsset json =  Resources.Load<TextAsset>("Json/AzureAPI");
            
            azureDataModel = JsonUtility.FromJson<AzureDataModel>(json.text);
        }
    }
}