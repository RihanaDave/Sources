using System;

namespace GPAS.Utility
{
    public class FileHash
    {
        public FileHash(byte[] hashBytes)
        {
            HashBytes = hashBytes;
            ComputeHashCode();
        }

        private void ComputeHashCode()
        {
            foreach (var item in HashBytes)
            {
                HashCode = HashCode ^ item;
            }
        }

        byte[] HashBytes;
        int HashCode;
        public override int GetHashCode()
        {
            return HashCode;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FileHash))
            {
                return false;
            }
            else
            {
                for (int i = 0; i < HashBytes.Length; i++)
                {
                    if (!(HashBytes[i].Equals(((FileHash)obj).HashBytes[i])))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}