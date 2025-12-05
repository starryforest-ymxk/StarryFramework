using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using StarryFramework;

/// <summary>
/// Resource Module 使用示例
/// 展示了Resources和Addressables两种加载方式的使用方法
/// </summary>
public class TestResource : MonoBehaviour
{
    private AsyncOperationHandle<GameObject> playerHandle;
    private AsyncOperationHandle<AudioClip> audioHandle;

    void Start()
    {
        // // 演示Resources加载方式
        // ResourcesLoadingExamples();
        //
        // // 演示Addressables加载方式
        // AddressablesLoadingExamples();
        
        StartCoroutine(AddressablesMultiLoadExample());
    }

    /// <summary>
    /// Resources加载方式示例
    /// </summary>
    void ResourcesLoadingExamples()
    {
        // 同步加载预制体（不实例化）
        GameObject prefab = Framework.ResourceComponent.LoadRes<GameObject>("Prefabs/Player");

        // 同步加载预制体并自动实例化
        GameObject instance = Framework.ResourceComponent.LoadRes<GameObject>("Prefabs/Enemy", true);

        // 加载文件夹下所有Sprite
        Sprite[] allSprites = Framework.ResourceComponent.LoadAllRes<Sprite>("Sprites/UI");

        // 异步加载音频
        Framework.ResourceComponent.LoadResAsync<AudioClip>("Audio/BGM", (clip) =>
        {
            if (clip != null)
            {
                Debug.Log($"加载音频成功: {clip.name}");
            }
        });

        // 异步加载并自动实例化
        Framework.ResourceComponent.LoadResAsync<GameObject>("Prefabs/Boss", (obj) =>
        {
            if (obj != null)
            {
                Debug.Log($"实例化Boss成功: {obj.name}");
            }
        }, true);
    }

    /// <summary>
    /// Addressables加载方式示例
    /// </summary>
    void AddressablesLoadingExamples()
    {
        // 同步加载（会阻塞主线程，不推荐）
        GameObject prefab = Framework.ResourceComponent.LoadAddressable<GameObject>("Player");

        // 同步加载并自动实例化
        GameObject instance = Framework.ResourceComponent.LoadAddressable<GameObject>("Enemy", true);

        // 异步加载音频
        audioHandle = Framework.ResourceComponent.LoadAddressableAsync<AudioClip>("BGM_Main", (clip) =>
        {
            if (clip != null)
            {
                Debug.Log($"加载Addressable音频成功: {clip.name}");
            }
        });

        // 异步加载预制体
        playerHandle = Framework.ResourceComponent.LoadAddressableAsync<GameObject>("PlayerCharacter", (obj) =>
        {
            if (obj != null)
            {
                Debug.Log($"加载Addressable预制体成功: {obj.name}");
            }
        }, true);

        // 使用Addressables专用实例化方法（推荐）
        Framework.ResourceComponent.InstantiateAddressable("SpawnableEnemy", transform);
    }

    /// <summary>
    /// Resources资源卸载示例
    /// </summary>
    void ResourcesUnloadingExamples()
    {
        // 卸载单个资源
        Texture2D texture = Framework.ResourceComponent.LoadRes<Texture2D>("Textures/Icon");
        Framework.ResourceComponent.UnloadRes(texture);

        // 卸载所有未使用的Resources资源
        Framework.ResourceComponent.UnloadUnusedRes();
    }

    /// <summary>
    /// Addressables资源释放示例
    /// </summary>
    void AddressablesUnloadingExamples()
    {
        // 释放异步操作句柄
        Framework.ResourceComponent.ReleaseAddressableHandle(playerHandle);

        // 释放资源对象
        AudioClip clip = Framework.ResourceComponent.LoadAddressable<AudioClip>("SFX_Explosion");
        Framework.ResourceComponent.ReleaseAddressableAsset(clip);

        // 释放实例化的GameObject
        GameObject instance = Framework.ResourceComponent.LoadAddressable<GameObject>("Projectile", true);
        Framework.ResourceComponent.ReleaseAddressableInstance(instance);

        // 释放所有Addressable句柄
        Framework.ResourceComponent.ReleaseAllAddressableHandles();
    }
    
    IEnumerator AddressablesMultiLoadExample()
    {
        Debug.Log("=== 开始第一次加载 ===");
        AsyncOperationHandle<GameObject> firstHandle = Framework.ResourceComponent.LoadAddressableAsync<GameObject>("TestPrefab", (asset) =>
        {
            Debug.Log($"第一次加载回调: {asset?.name}");
        });
    
        Debug.Log($"第一次加载句柄创建: IsValid={firstHandle.IsValid()}, HashCode={firstHandle.GetHashCode()}");
    
        yield return firstHandle;
    
        Debug.Log($"第一次加载完成: Status={firstHandle.Status}, IsValid={firstHandle.IsValid()}");
    
        Debug.Log("=== 释放第一次句柄 ===");
        Framework.ResourceComponent.ReleaseAddressableHandle(firstHandle);
    
        Debug.Log($"释放后句柄状态: IsValid={firstHandle.IsValid()}");
    
        yield return new WaitForSeconds(0.1f);
    
        Debug.Log("=== 开始第二次加载 ===");
        AsyncOperationHandle<GameObject> secondHandle = Framework.ResourceComponent.LoadAddressableAsync<GameObject>("TestPrefab", (asset) =>
        {
            Debug.Log($"第二次加载回调: {asset?.name}");
        });
    
        Debug.Log($"第二次加载句柄创建: IsValid={secondHandle.IsValid()}, HashCode={secondHandle.GetHashCode()}");
    
        yield return secondHandle;
    
        Debug.Log($"第二次加载完成前检查: IsValid={secondHandle.IsValid()}");
    
        if (secondHandle.IsValid())
        {
            Debug.Log($"✅ 第二次加载状态: {secondHandle.Status}");
            Debug.Log($"✅ 测试成功！第二次加载的句柄有效");
        }
        else
        {
            Debug.LogError("❌ 第二次句柄无效！");
        }
        
        Framework.ResourceComponent.ReleaseAddressableHandle(secondHandle);
    }



    void OnDestroy()
    {
        // 组件销毁时释放持有的Addressable句柄
        if (playerHandle.IsValid())
        {
            Framework.ResourceComponent.ReleaseAddressableHandle(playerHandle);
        }

        if (audioHandle.IsValid())
        {
            Framework.ResourceComponent.ReleaseAddressableHandle(audioHandle);
        }
    }
}
