using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prot
{
    internal class FileGenerator
    {
        private static Random random = new Random();

        public static void GenerateFile(string path, string filename)
        {
            // Генерация уникального имени файла

            string fullpath = System.IO.Path.Combine(path, filename);

            using (StreamWriter writer = new StreamWriter(fullpath))
            {
                for (int i = 1; i <= 100000; i++)
                {
                    string line = GenerateLine();
                    writer.WriteLine(line);
                }
            }
        }

        public static void MergeFiles(string outputFileName, string patternToRemove, string basePath)
        {
            try
            {
                int totalRemovedLines = 0;
                string fullpath = System.IO.Path.Combine(basePath, outputFileName);
                using (StreamWriter mergedWriter = new StreamWriter(fullpath))
                {
                    for (int i = 1; i <= 100; i++)
                    {
                        string inputFileName = System.IO.Path.Combine(basePath, $"output_{i}.txt");
                        if (File.Exists(inputFileName))
                        {
                            string[] lines = File.ReadAllLines(inputFileName);
                            foreach (string line in lines)
                            {
                                if (string.IsNullOrEmpty(patternToRemove) || !line.Contains(patternToRemove))
                                {
                                    mergedWriter.WriteLine(line);
                                }
                                else
                                {
                                    totalRemovedLines++;
                                }
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show($"Файл {inputFileName} не найден.");
                        }
                    }
                }

                string resultMessage = string.IsNullOrEmpty(patternToRemove)
                    ? $"Объединение файлов завершено. Результат в файле {outputFileName}"
                    : $"Объединение файлов завершено. Удалено {totalRemovedLines} строк с паттерном '{patternToRemove}'. Результат в файле {outputFileName}";

                System.Windows.MessageBox.Show(resultMessage);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Произошла ошибка при объединении файлов: {ex.Message}");
            }
        }

        private static string GenerateLine()
        {
            DateTime randomDate = DateTime.Now.AddYears(-random.Next(1, 6));
            string latinChars = GenerateRandomString(10, "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
            string russianChars = GenerateRandomString(10, "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя");
            int evenInt = random.Next(1, 50000000) * 2;
            double decimalNumber = Math.Round(random.NextDouble() * 19 + 1, 8);

            string line = $"{randomDate.ToString("dd.MM.yyyy")}||{latinChars}||{russianChars}||{evenInt}||{decimalNumber}||";
            return line;
        }

        private static string GenerateRandomString(int length, string characters)
        {
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = characters[random.Next(characters.Length)];
            }
            return new string(result);
        }

    }
}
