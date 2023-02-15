using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace FileDownload;
public static class Programm
{
    static async Task Main()
    {
        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("gzip"));
        try
        {
            var response = await httpClient.GetAsync("https://opencellid.org/ocid/downloads?token=pk.add17ff360f6360eb68bbd24983b7e06&type=mcc&file=257.csv.gz");
            using var ms = new MemoryStream();
            await response.Content.CopyToAsync(ms);
            byte[] data;
            data = await GZip(ms);
            using var msData = new MemoryStream(data);

            string patternInt = @"^\d+$";
            string patternFloat = @"-?\d{1,3}\.\d+";
            using StreamReader reader = new StreamReader(msData);
            Console.WriteLine(reader);
            using StreamWriter writer = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Data3.csv");
            while (reader.Peek() != -1)
            {
                int amountElements = 1;
                int startIndex = 0;
                string? textLine = reader.ReadLine();
                int index = textLine.IndexOf(',', startIndex);
                Console.WriteLine(textLine);
                if (index != -1)
                {
                    if (textLine.AsSpan().Slice(startIndex, index - startIndex).ToString() != "GSM")
                    {
                        continue;
                    }

                    amountElements++;
                    startIndex = index + 1;
                }

                var line = new StringBuilder();
                bool check = true;
                while (textLine.IndexOf(',', startIndex) != -1 && check)
                {
                    index = textLine.IndexOf(',', startIndex);
                    if (amountElements == 2 || amountElements == 3 || amountElements == 4 || amountElements == 5)
                    {
                        var world = textLine.AsSpan().Slice(startIndex, index - startIndex).ToString();
                        if (Regex.IsMatch(world, patternInt))
                        {
                            line.Append(world);
                            if (amountElements != 8)
                            {
                                line.Append(',');
                            }
                        }
                        else
                        {
                            check = false;
                            break;
                        }
                    }
                    else if (amountElements == 7 || amountElements == 8)
                    {
                        var world = textLine.AsSpan().Slice(startIndex, index - startIndex).ToString();
                        if (Regex.IsMatch(world, patternFloat))
                        {
                            line.Append(world);
                            if (amountElements != 8)
                            {
                                line.Append(',');
                            }
                        }
                        else
                        {
                            check = false;
                            break;
                        }
                    }

                    amountElements++;
                    startIndex = index + 1;
                }

                if (check)
                {
                    writer.WriteLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return;
        }
    }

    private static async Task<byte[]> GZip(MemoryStream inputStream, CancellationToken cancel = default)
    {
        inputStream.Position = 0;
        using (var outputStream = new MemoryStream())
        {
            using (var compressionStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                await compressionStream.CopyToAsync(outputStream, cancel);
            }
            return outputStream.ToArray();
        }
    }
}