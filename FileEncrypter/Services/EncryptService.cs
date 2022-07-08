namespace FileEncrypter.Services;

public static class EncryptService
{
    public static void EncryptByteArray(ref byte[] array, uint key)
    {
        ArgumentNullException.ThrowIfNull(array);

        var advancedKey = Convert.ToByte(key % byte.MaxValue);

        for (byte i = 0; i < array.Length; i++)
        {
            array[i] += advancedKey;
            array[i] ^= advancedKey;
        }
            
    }

    public static void DecryptByteArray(ref byte[] array, uint key)
    {
        ArgumentNullException.ThrowIfNull(array);

        var advancedKey = Convert.ToByte(key % byte.MaxValue);

        for (int i = 0; i < array.Length; i++)
        {
            array[i] ^= advancedKey;
            array[i] -= advancedKey;
        }
            
    }
}
