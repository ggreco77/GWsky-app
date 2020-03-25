using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public static class HashExtension { 
    // The cryptographic service provider.
    static readonly MD5 MD5 = MD5.Create();

    public static string GetHashMD5(string filename) {
        using (FileStream stream = File.OpenRead(filename)) {
            var hash = MD5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
