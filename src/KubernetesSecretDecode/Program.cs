using System;

namespace KubernetesSecretDecode
{
    class Program
    {
        static int Main(string[] args)
        {
            var decoder = new Decoder(args);
            try
            {
                var result = decoder.Decode();
                Console.WriteLine(result);
            }
            catch (DecoderException e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }

            return 0;
        }
    }
}