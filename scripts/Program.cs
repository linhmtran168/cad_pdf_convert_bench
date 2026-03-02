// Generates a strong name key pair (SNK) in the Microsoft CAPI blob format.
// Usage: dotnet run -- [output-directory]
// Outputs: MyKeyPair.snk, MyPublicKey.snk, and prints the public key token.

using System.Security.Cryptography;

var outputDir = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
Directory.CreateDirectory(outputDir);

var keyPairPath = Path.Combine(outputDir, "MyKeyPair.snk");
var publicKeyPath = Path.Combine(outputDir, "MyPublicKey.snk");

using var rsa = RSA.Create(1024);
var p = rsa.ExportParameters(true);

// CAPI PRIVATEKEYBLOB format
using var ms = new MemoryStream();
using var bw = new BinaryWriter(ms);
bw.Write((byte)0x07);    // bType = PRIVATEKEYBLOB
bw.Write((byte)0x02);    // bVersion
bw.Write((ushort)0);     // reserved
bw.Write(0x00002400);    // aiKeyAlg = CALG_RSA_SIGN
bw.Write(0x32415352);    // magic = "RSA2"
bw.Write(p.Modulus!.Length * 8); // bitlen
bw.Write(ToUint(p.Exponent!));
bw.Write(Rev(p.Modulus));
bw.Write(Rev(p.P!));
bw.Write(Rev(p.Q!));
bw.Write(Rev(p.DP!));
bw.Write(Rev(p.DQ!));
bw.Write(Rev(p.InverseQ!));
bw.Write(Rev(p.D!));

File.WriteAllBytes(keyPairPath, ms.ToArray());
Console.WriteLine($"Key pair written to:  {keyPairPath}");

// CAPI PUBLICKEYBLOB format
using var pubMs = new MemoryStream();
using var pubBw = new BinaryWriter(pubMs);
pubBw.Write((byte)0x06); // bType = PUBLICKEYBLOB
pubBw.Write((byte)0x02); // bVersion
pubBw.Write((ushort)0);  // reserved
pubBw.Write(0x00002400); // aiKeyAlg = CALG_RSA_SIGN
pubBw.Write(0x31415352); // magic = "RSA1"
pubBw.Write(p.Modulus.Length * 8);
pubBw.Write(ToUint(p.Exponent));
pubBw.Write(Rev(p.Modulus));
var pubBlob = pubMs.ToArray();

// .NET strong name public key wrapper
using var snMs = new MemoryStream();
using var snBw = new BinaryWriter(snMs);
snBw.Write(0x00002400);  // SigAlgId = CALG_RSA_SIGN
snBw.Write(0x00008004);  // HashAlgId = CALG_SHA1
snBw.Write(pubBlob.Length);
snBw.Write(pubBlob);
var snPublicKey = snMs.ToArray();

File.WriteAllBytes(publicKeyPath, snPublicKey);
Console.WriteLine($"Public key written to: {publicKeyPath}");

// Public key token = last 8 bytes of SHA-1(public key), reversed
var hash = SHA1.HashData(snPublicKey);
var token = new byte[8];
Array.Copy(hash, hash.Length - 8, token, 0, 8);
Array.Reverse(token);
Console.WriteLine($"Public key token:      {Convert.ToHexString(token).ToLowerInvariant()}");

static byte[] Rev(byte[] d) { var c = (byte[])d.Clone(); Array.Reverse(c); return c; }
static uint ToUint(byte[] d) { uint v = 0; foreach (var b in d) v = (v << 8) | b; return v; }
