<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Numerics.dll</Reference>
  <NuGetReference>BouncyCastle</NuGetReference>
  <NuGetReference>Castle.Core</NuGetReference>
  <Namespace>Org.BouncyCastle.Asn1.X509</Namespace>
  <Namespace>Org.BouncyCastle.Crypto</Namespace>
  <Namespace>Org.BouncyCastle.Crypto.Generators</Namespace>
  <Namespace>Org.BouncyCastle.Crypto.Prng</Namespace>
  <Namespace>Org.BouncyCastle.Security</Namespace>
  <Namespace>Org.BouncyCastle.X509</Namespace>
  <Namespace>System.Security.Cryptography.X509Certificates</Namespace>
  <Namespace>Org.BouncyCastle.Math</Namespace>
</Query>

void Main()
{
	var x = GenerateCertificate("CoreDocker.Dev");
	x.RawData.Length.Dump();
}

static X509Certificate2 GenerateCertificate(string certName)
{
	var keypairgen = new RsaKeyPairGenerator();
	keypairgen.Init(new KeyGenerationParameters(new SecureRandom(new CryptoApiRandomGenerator()), 1024));

	var keypair = keypairgen.GenerateKeyPair();

	var gen = new X509V3CertificateGenerator();

	var CN = new X509Name("CN=" + certName);
	var SN = BigInteger.ProbablePrime(120, new Random());

	gen.SetSerialNumber(SN);
	gen.SetSubjectDN(CN);
	gen.SetIssuerDN(CN);
	gen.SetNotAfter(DateTime.MaxValue);
	gen.SetNotBefore(DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0)));
	gen.SetSignatureAlgorithm("MD5WithRSA");
	gen.SetPublicKey(keypair.Public);

	var newCert = gen.Generate(keypair.Private);

	return new X509Certificate2(DotNetUtilities.ToX509Certificate((Org.BouncyCastle.X509.X509Certificate)newCert));
}

// Define other methods and classes here
