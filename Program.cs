using Firebase.Storage;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System.Text;

string Bucket = "uploadfilefirebase-b465e.appspot.com";
var fileName = "file";
string filePath = "D:\\project\\UploadFileFireBase\\file.txt";
string jsonPath = "D:\\project\\UploadFileFireBase\\Credential.json";
var credential = GoogleCredential.FromFile(jsonPath);
var folderName = "Files";
var fileNameWithFolder = $"{folderName}/{fileName}";

//await UploadImageToFolderAsync();
//await DeleteImageToFolderAsync();

await UpdateFileToFolderAsync();

async Task UpdateFileToFolderAsync()
{
    var storage = StorageClient.Create(credential);

    // Get a reference to the FirebaseStorage
    var firebaseStorage = new FirebaseStorage(Bucket);

    // Get a reference to the file you want to download
    var fileReference = firebaseStorage
                        .Child(folderName)
                        .Child(fileName);

    // Get the download URL for the file
    var downloadUrl = await fileReference.GetDownloadUrlAsync();

    // Use an HttpClient to download the file
    var httpClient = new HttpClient();
    string serverContent = await httpClient.GetStringAsync(downloadUrl);

    // Read the new content from the local file
    string localContent = File.ReadAllText(filePath);

    // Append the new content to the server content
    serverContent += localContent;

    // Now you can upload the updated content to Firebase Storage
    var stream = new MemoryStream(Encoding.UTF8.GetBytes(serverContent));

    var metadata = new Google.Apis.Storage.v1.Data.Object
    {
        ContentType = "text/plain",
        Name = fileNameWithFolder,
        Bucket = Bucket
    };

    var task = await storage.UploadObjectAsync(metadata, stream);
    string url = $"https://firebasestorage.googleapis.com/v0/b/{task.Bucket}/o/{task.Name}?alt=media";
    Console.WriteLine("file uploaded successfully! \n"  + task.MediaLink);

}


async Task UploadImageToFolderAsync()
{
    var storage = StorageClient.Create(credential);
    using var fileStream = File.Open(filePath, FileMode.Open);

    #region MyRegion
    //var metadata = new Google.Apis.Storage.v1.Data.Object
    //{
    //    ContentType = "image/png",
    //    Name = objectNameWithFolder,
    //    Bucket = Bucket
    //};

    //var task = await storage.UploadObjectAsync(metadata, fileStream);
    //string url = $"https://firebasestorage.googleapis.com/v0/b/{task.Bucket}/o/{task.Name}?alt=media";
    //Console.WriteLine("Uploaded file URL:" + url); 
    #endregion

    try
    {
        var obj = await storage.GetObjectAsync(Bucket, fileNameWithFolder);
        string url = $"https://firebasestorage.googleapis.com/v0/b/{Bucket}/o/{fileName}?alt=media";
        Console.WriteLine("Image already exist." + url);
    }
    catch (Google.GoogleApiException e) when (e.Error.Code == 404)
    {
        var metadata = new Google.Apis.Storage.v1.Data.Object
        {
            //ContentType = "image/png",
            ContentType = "text/plain",
            Name = fileNameWithFolder,
            Bucket = Bucket
        };

        var task = await storage.UploadObjectAsync(metadata, fileStream);
        string url = $"https://firebasestorage.googleapis.com/v0/b/{task.Bucket}/o/{task.Name}?alt=media";
        Console.WriteLine("Image uploaded successfully!" + url);
    }
}

async Task DeleteImageToFolderAsync()
{
    var storage = new FirebaseStorage(Bucket);

    var imageRef = storage.Child(fileNameWithFolder);
    try
    {
        var url = await imageRef.GetDownloadUrlAsync();
        var task = storage.Child(folderName).Child(fileName);
        await task.DeleteAsync();
        Console.WriteLine("Image deleted successfully!");
    }
    catch (FirebaseStorageException)
    {
        Console.WriteLine("Image does not exist.");
    }

    #region MyRegion
    //var deleteImg = storage.Child(folderName).Child(fileName);
    //await deleteImg.DeleteAsync();
    //Console.WriteLine("Image deleted successfully!"); 
    #endregion
}
