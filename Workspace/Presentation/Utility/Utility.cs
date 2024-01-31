using System;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Media.Imaging;
namespace GPAS.Workspace.Presentation.Utility
{
    public class Utility
    {
        public static void Serialize<T>(Stream stream, T objectToSerialize)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (objectToSerialize == null)
                throw new ArgumentNullException(nameof(objectToSerialize));

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            xmlSerializer.Serialize(stream, (T)objectToSerialize);
        }

        public static void SerializeToFile<T>(string filePath, T objectToSerialize)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            if (objectToSerialize == null)
                throw new ArgumentNullException(nameof(objectToSerialize));

            using (StreamWriter streamWriter = new StreamWriter(filePath))
            {
                Serialize(streamWriter.BaseStream, objectToSerialize);
            }
        }

        public static T DeSerialize<T>(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(stream);
        }

        public static T DeSerializeFromFile<T>(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException(nameof(filePath));

            T deserializedObject;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                deserializedObject = DeSerialize<T>(streamReader.BaseStream);
            }

            return deserializedObject;
        }

        public static BitmapImage GetIconResource(string title)
        {
            System.Windows.ResourceDictionary iconsResource = new System.Windows.ResourceDictionary
            {
                Source = new Uri("/Resources/Icons.xaml", UriKind.Relative)
            };

            BitmapImage icon = iconsResource[title] as BitmapImage;
            icon?.Freeze();
            return icon;
        }

        public static void CreateShortcut(string targetPath, string shortcutFile, string description,
            string arguments)
        {
            // Check necessary parameters first:
            if (string.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException(nameof(targetPath));

            if (string.IsNullOrEmpty(shortcutFile))
                throw new ArgumentNullException(nameof(shortcutFile));             
            
            File.WriteAllBytes(shortcutFile, new byte[0]);
            // Create a ShellLinkObject that references the .lnk file
            Shell32.Shell shl = new Shell32.Shell();
            Shell32.Folder dir = shl.NameSpace(Path.GetDirectoryName(shortcutFile));
            Shell32.FolderItem itm = dir.Items().Item(Path.GetFileName(shortcutFile));
            Shell32.ShellLinkObject lnk = (Shell32.ShellLinkObject)itm.GetLink;
            // Set the .lnk file properties
            lnk.Path = targetPath;
            lnk.Description = description;
            lnk.Arguments = arguments;
            lnk.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            lnk.Save(shortcutFile);
        }
    }
}
