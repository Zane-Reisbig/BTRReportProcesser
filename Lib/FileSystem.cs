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
        FolderPicker FolderPicker;
        FileOpenPicker FileOpenPicker;
        UserInformation user;

        private async Task<FileSystem> Init()
        {
            await user.Init;
            return this;
        }

        private FileSystem()
        {
            user = new UserInformation();
            FolderPicker = new FolderPicker();
            FileOpenPicker = new FileOpenPicker();
        }

        public static async Task<FileSystem> CreateAsync()
        {
            var ret = new FileSystem();
            return await ret.Init();

        }

        public async Task<StorageFolder> RequestOrGetFolder(string futureListSaveName, FutureItemListOptions option = FutureItemListOptions.ReturnFutureItemIfExists)
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem(futureListSaveName) && option == FutureItemListOptions.ReturnFutureItemIfExists)
            {
                StorageFolder ret = await StorageApplicationPermissions.FutureAccessList.GetItemAsync( futureListSaveName ) as StorageFolder;
                return ret;
            }

            FolderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            FolderPicker.CommitButtonText = "Save Location";
            FolderPicker.FileTypeFilter.Add("*");

            var outFolder = await FolderPicker.PickSingleFolderAsync();
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
        
        public bool HaveAccessTo(string daPath)
        {

            if (StorageApplicationPermissions.FutureAccessList.ContainsItem(daPath)) return true;

            return false;

        }

        public async Task<StorageFile> GetSingleFile(PickerLocationId start, string[] files) { return await GetSingleFile(start, files.ToList()); }
        public async Task<StorageFile> GetSingleFile(PickerLocationId start, List<string> files)
        {
            FileOpenPicker.SuggestedStartLocation = start;
            foreach(string fileName in files)
            {
                FileOpenPicker.FileTypeFilter.Add(fileName);
            }

            return await FileOpenPicker.PickSingleFileAsync(); 
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

            StorageFolder selected = (StorageFolder) await RequestOrGetFolder(futureAccessLabel);
            if (selected == null) return false;

            outFile = await selected.CreateFileAsync(fileName, option);
            writer = new StreamWriter(await outFile.OpenStreamForWriteAsync());
            writer.Write(content);
            writer.Close();
            return true;

        }

    }
}
