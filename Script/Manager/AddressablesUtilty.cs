using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressablesUtilty
{
    public static Task<T> LoadAssetObjectAsyncTask<T>(string key)
    {
        TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
        AsyncOperationHandle<T> loadAssetHandle = Addressables.LoadAssetAsync<T>(key);

        loadAssetHandle.Completed += (AsyncOperationHandle<T> completedHandle) =>
        {
            if (completedHandle.Status == AsyncOperationStatus.Failed)
            {
                AssetRelease(loadAssetHandle);
            }

            taskCompletionSource.SetResult(completedHandle.Result);
        };

        return taskCompletionSource.Task;
    }

    public static void AssetRelease<T>(T releaseAsset)
    {
        Addressables.Release(releaseAsset);
    }
}