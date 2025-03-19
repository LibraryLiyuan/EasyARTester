using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using PackagesEasyAR.Util;

public class TestAARemote : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        Debug.Log(System.IO.Directory.GetCurrentDirectory()); // 输出：E:\kingBook\projects\unity_swfParse ，注意此方法可能通过 Directory.SetCurrentDirectory(path); 进行设置
        Debug.Log(System.Environment.CurrentDirectory); //获取到本地工程的绝对路径，如：E:\kingBook\projects\unity_swfParse
        Debug.Log(Application.dataPath); //Assets资源文件夹的绝对路径，如：E:/kingBook/projects/unity_swfParse/Assets
        Debug.Log(Application.persistentDataPath); //持久性的数据存储路径，在不同平台路径不同，但都存在，绝对路径，如：C:/Users/Administrator/AppData/LocalLow/DefaultCompany/unity_swfParse
        Debug.Log(Application.streamingAssetsPath); //Assets资源文件夹下StreamingAssets文件夹目录的绝对路径，如：E:/kingBook/projects/unity_swfParse/Assets/StreamingAssets
        Debug.Log(Application.temporaryCachePath); //游戏运行时的缓存目录，也是绝对路径，如：C:/Users/ADMINI~1/AppData/Local/Temp/DefaultCompany/unity_swfParse
    }
}
