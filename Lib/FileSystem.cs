using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace BTRReportProcesser.Lib
{
    enum FutureItemListOptions : int
    {
        ReplaceFutureItemIfExists = 0,
        ReturnFutureItemIfExists = 1
    }

    internal class FileSystem
    {
        FolderPicker folderPicker;
        UserInformation user;

        private async Task<FileSystem> Init()
        {
            await user.Init;
            return this;
        }

        private FileSystem()
        {
            user = new UserInformation();
            folderPicker = new Windows.Storage.Pickers.FolderPicker();
        }

        public static async Task<FileSystem> CreateAsync()
        {
            var ret = new FileSystem();
            return await ret.Init();

        }

        public async Task<StorageFolder> RequestFolderAccess(string futureListSaveName, FutureItemListOptions option = FutureItemListOptions.ReturnFutureItemIfExists)
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem(futureListSaveName) && option == FutureItemListOptions.ReturnFutureItemIfExists)
            {
                StorageFolder ret = await StorageApplicationPermissions.FutureAccessList.GetItemAsync( futureListSaveName ) as StorageFolder;
                return ret;
            }

            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.CommitButtonText = "Save Location";
            folderPicker.FileTypeFilter.Add("*");

            var outFolder = await folderPicker.PickSingleFolderAsync();
            if (outFolder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.AddOrReplace(futureListSaveName, outFolder);
                return outFolder;
            }

            return null;

        }

        public async Task<bool> WriteFile(string fileName, CreationCollisionOption option, string content, string futureAccessLabel, StorageFolder tryTargetContainer = null)
        {
            StorageFile outFile;
            StreamWriter writer;
            if(tryTargetContainer != null)
            {
                if (StorageApplicationPermissions.FutureAccessList.ContainsItem(futureAccessLabel))
                {
                    try
                    {
                        outFile = await tryTargetContainer.CreateFileAsync(fileName, option);
                        writer = new StreamWriter(await outFile.OpenStreamForWriteAsync());
                        writer.Write(content);
                        writer.Close();

                        return true;
                    }
                    catch(SecurityAccessDeniedException e)
                    {
                        // If it fails it fails
                        // onwards to the file explorer
                        // TODO: Make this method ?recursive maybe?
                        // try writeFile(selectedFolder, ...)

                        e.ToString();
                    }

                }
            }

            StorageFolder selected = (StorageFolder) await RequestFolderAccess(futureAccessLabel);
            if (selected == null) return false;

            outFile = await selected.CreateFileAsync(fileName, option);
            writer = new StreamWriter(await outFile.OpenStreamForWriteAsync());
            writer.Write(content);
            writer.Close();
            return true;

        }

    }
}
