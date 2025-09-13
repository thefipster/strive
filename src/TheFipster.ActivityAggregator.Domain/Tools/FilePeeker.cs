using System.Text;
using System.Text.Json;

namespace TheFipster.ActivityAggregator.Domain.Tools
{
    public class FilePeeker
    {
        private readonly string filepath;

        public FilePeeker(string filepath)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException(filepath);

            this.filepath = filepath;
        }

        /// <summary>
        /// Reads the first n characters of the given file.
        /// </summary>
        /// <param name="charCount">Number of characters read from file</param>
        /// <returns>First n characters as string</returns>
        public string ReadChars(int charCount)
        {
            using (var stream = File.OpenRead(filepath))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                char[] buffer = new char[charCount];
                int n = reader.ReadBlock(buffer, 0, charCount);
                char[] result = new char[n];
                Array.Copy(buffer, result, n);

                return new string(result);
            }
        }

        /// <summary>
        /// Returns the value of the given property if found before read limit is reached.
        /// </summary>
        /// <param name="propertyName">Property to search for</param>
        /// <param name="readLimit">Number of iterations. Every interation reads 8 kB.</param>
        /// <returns>Value of the given Property of null if not found.</returns>
        public string? ReadTokens(string propertyName, int readLimit = int.MaxValue)
        {
            using FileStream fs = File.OpenRead(filepath);
            var reader = new Utf8JsonReader(new ReadOnlySpan<byte>());
            byte[] buffer = new byte[8192]; // stream buffer
            int bytesRead;
            int loopCount = 0;
            bool isProperty = false;

            while ((bytesRead = fs.Read(buffer, 0, buffer.Length)) > 0 && loopCount < readLimit)
            {
                loopCount++;
                var span = new ReadOnlySpan<byte>(buffer, 0, bytesRead);

                reader = new Utf8JsonReader(
                    span,
                    isFinalBlock: bytesRead < buffer.Length,
                    reader.CurrentState
                );

                while (reader.Read())
                {
                    if (
                        reader.TokenType == JsonTokenType.PropertyName
                        && reader.ValueTextEquals(propertyName)
                    )
                    {
                        isProperty = true;
                    }
                    else if (isProperty)
                    {
                        if (reader.TokenType == JsonTokenType.String)
                            return reader.GetString();

                        if (reader.TokenType == JsonTokenType.Number)
                            return reader.GetDouble().ToString();

                        if (
                            reader.TokenType == JsonTokenType.True
                            || reader.TokenType == JsonTokenType.False
                        )
                            return reader.GetBoolean().ToString();
                    }
                }
            }

            return null; // not found
        }

        /// <summary>
        /// Returns the first n lines of the given file.
        /// </summary>
        /// <param name="lineCount">Number of lines to be read.</param>
        /// <returns>First n lines of file.</returns>
        public IEnumerable<string> ReadLines(int lineCount) =>
            File.ReadLines(filepath).Take(lineCount).ToList();
    }
}
