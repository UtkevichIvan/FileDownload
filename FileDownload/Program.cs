using System.IO.Compression;
using System.Text;

namespace FileDownload;

public static class Programm
{
    static async Task Main()
    {
        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("accept-encoding", "gzip");
        try
        {
            var response = await httpClient.GetAsync("https://opencellid.org/ocid/downloads?token=pk.ca5c8ab70ab675e44e3fdad9d63215a1&type=mcc&file=257.csv.gz");
            using var responseStream = await response.Content.ReadAsStreamAsync();
            using var decompressed = new GZipStream(responseStream, CompressionMode.Decompress);

            using StreamReader reader = new StreamReader(decompressed);
            Console.WriteLine(reader.ReadToEnd());
            using StreamWriter writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Data2.csv");
            while (reader.Peek() != -1)
            {
                bool check = true;
                int amountElements = 1;
                var line = new StringBuilder();
                char symbol = (char)reader.Read();
                while (symbol != ',')
                {
                    line.Append(symbol);
                    symbol = (char)reader.Read();
                }

                if (!line.Equals("GSM"))
                {
                    check = false;
                }

                line.Append(',');
                amountElements++;
                symbol = (char)reader.Read();

                while ((amountElements == 2 || amountElements == 3 || amountElements == 4 || amountElements == 5) && check)
                {
                    while (symbol != ',')
                    {
                        if (Char.IsDigit(symbol))
                        {
                            line.Append(symbol);
                            symbol = (char)reader.Read();
                        }
                        else
                        {
                            check = false;
                        }
                    }

                    if (!check)
                    {
                        continue;
                    }

                    line.Append(',');
                    amountElements++;
                    symbol = (char)reader.Read();
                }

                while (symbol != ',')
                {
                    symbol = (char)reader.Read();
                }

                symbol = (char)reader.Read();
                amountElements++;

                while ((amountElements == 7 || amountElements == 8) && check)
                {
                    int dotsAmount = 0;
                    while (symbol != ',')
                    {
                        if (Char.IsDigit(symbol) || symbol == '.')
                        {
                            if (symbol == '.')
                            {
                                dotsAmount++;
                            }

                            line.Append(symbol);
                            symbol = (char)reader.Read();
                        }
                        else
                        {
                            check = false;
                        }
                    }
                    if (dotsAmount != 1)
                    {
                        check = false;
                    }

                    if (!check)
                    {
                        continue;
                    }

                    if (amountElements != 8)
                    {
                        line.Append(',');
                    }

                    amountElements++;
                    symbol = (char)reader.Read();
                }

                while (symbol != '\n' && reader.Peek() != -1)
                {
                    symbol = (char)reader.Read();
                }

                if (check)
                {
                    writer.Write(line);
                    writer.Write('\n');
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }
    }
}