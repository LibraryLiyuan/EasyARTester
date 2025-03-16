using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
public class TestAARemote : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        var item2 = await Addressables.LoadAssetAsync<GameObject>("Assets/Test/Cylinder.prefab").Task;
        GameObject.Instantiate(item2);
        
        var item = await Addressables.LoadAssetAsync<GameObject>("Assets/Test/Sphere.prefab").Task;
        GameObject.Instantiate(item);
    }

    // Update is called once per frame
    async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var item = await Addressables.LoadAssetAsync<GameObject>("Assets/Test/Cube.prefab").Task;
            GameObject.Instantiate(item);
        }
    }
}
