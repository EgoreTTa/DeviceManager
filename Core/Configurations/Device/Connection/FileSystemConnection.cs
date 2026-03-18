namespace Core.Configurations.Device.Connection
{
    using System;

    public class FileSystemConnection : IEquatable<FileSystemConnection>
    {
        public string FolderToRead { get; set; }
        public string FolderToWrite { get; set; }

        public bool Equals(FileSystemConnection other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return FolderToRead == other.FolderToRead && FolderToWrite == other.FolderToWrite;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FileSystemConnection)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FolderToRead, FolderToWrite);
        }
    }
}