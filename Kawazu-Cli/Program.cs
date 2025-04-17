using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kawazu
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            Console.WriteLine("Kawazu-Cli Japanese Converter Version 1.0.0");
            Console.WriteLine("Type 'exit' to quit");
            Console.WriteLine();
            
            using var converter = new KawazuConverter(); // Call KawazuConverter.Dispose() by using statement
            
            if (args.Length > 1)
            {
                bool nextInputFile = false;
                bool nextOutputFile = false;
                bool addPronunciation = false;
                string inputFileName = null;
                string outputFileName = null;
                To to = To.Romaji;
                Mode mode = Mode.Okurigana;
                RomajiSystem romajiSystem = RomajiSystem.Hepburn;
                foreach (var arg in args)
                {
                    switch (arg)
                    {
                        case "-i":
                            nextInputFile = true;
                            break;
                        case "-o":
                            nextOutputFile = true;
                            break;
                        case "-p":
                            addPronunciation = true;
                            break;
                        default:
                            if (nextInputFile)
                            {
                                inputFileName = arg;
                                nextInputFile = false;
                            }
                            else if (nextOutputFile)
                            {
                                outputFileName = arg;
                                nextOutputFile = true;
                            }
                            else
                            {
                                (to, mode, romajiSystem) = arg switch
                                {
                                    "-rnn" => (To.Romaji, Mode.Normal, RomajiSystem.Nippon),
                                    "-rnp" => (To.Romaji, Mode.Normal, RomajiSystem.Passport),
                                    "-rnh" => (To.Romaji, Mode.Normal, RomajiSystem.Hepburn),
                                    "-rsn" => (To.Romaji, Mode.Spaced, RomajiSystem.Nippon),
                                    "-rsp" => (To.Romaji, Mode.Spaced, RomajiSystem.Passport),
                                    "-rsh" => (To.Romaji, Mode.Spaced, RomajiSystem.Hepburn),
                                    "-ron" => (To.Romaji, Mode.Okurigana, RomajiSystem.Nippon),
                                    "-rop" => (To.Romaji, Mode.Okurigana, RomajiSystem.Passport),
                                    "-roh" => (To.Romaji, Mode.Okurigana, RomajiSystem.Hepburn),
                                    "-rfn" => (To.Romaji, Mode.Furigana, RomajiSystem.Nippon),
                                    "-rfp" => (To.Romaji, Mode.Furigana, RomajiSystem.Passport),
                                    "-rfh" => (To.Romaji, Mode.Furigana, RomajiSystem.Hepburn),
                                    "-hn" => (To.Hiragana, Mode.Normal, RomajiSystem.Hepburn),
                                    "-hs" => (To.Hiragana, Mode.Spaced, RomajiSystem.Hepburn),
                                    "-ho" => (To.Hiragana, Mode.Okurigana, RomajiSystem.Hepburn),
                                    "-hf" => (To.Hiragana, Mode.Furigana, RomajiSystem.Hepburn),
                                    "-kn" => (To.Katakana, Mode.Normal, RomajiSystem.Hepburn),
                                    "-ks" => (To.Katakana, Mode.Spaced, RomajiSystem.Hepburn),
                                    "-ko" => (To.Katakana, Mode.Okurigana, RomajiSystem.Hepburn),
                                    "-kf" => (To.Katakana, Mode.Furigana, RomajiSystem.Hepburn),
                                    _ => (To.Romaji, Mode.Okurigana, RomajiSystem.Hepburn)
                                };
                            }
                            break;
                    }
                }
                if (string.IsNullOrWhiteSpace(inputFileName))
                {
                    Console.WriteLine("Specify input file with -i option");
                    return;
                }
                var lines = new List<string>();
                foreach (var line in File.ReadAllLines(inputFileName))
                {
                    lines.Add(converter.Convert(line, to, mode, romajiSystem, "(", ")").GetAwaiter().GetResult());
                    if (addPronunciation)
                    {
                        var pronunciation = new StringBuilder();
                        foreach (var div in await converter.GetDivisions(line, to, mode, romajiSystem, "(", ")"))
                        {
                            pronunciation.Append(div.RomaPronunciation);
                        }
                        lines.Add(pronunciation.ToString());
                    }
                }
                if (!string.IsNullOrWhiteSpace(outputFileName))
                {
                    File.AppendAllLines(outputFileName, lines);
                }
                else
                {
                    foreach (var line in lines)
                    {
                        Console.WriteLine(line);
                    }
                }
            }
            else
            while (true)
            {
                Console.WriteLine("Original Japanese Sentence:");
                Console.Write("> ");
                var str = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(str))
                {
                    continue;
                }

                if (str == "exit")
                {
                    return;
                }
                
                Console.WriteLine("Target form ('1':Romaji '2':Hiragana '3':Katakana Default:Hiragana):");
                Console.Write("> ");
                var toStr = Console.ReadLine();
                var to = toStr switch
                {
                    "1" => To.Romaji,
                    "2" => To.Hiragana,
                    "3" => To.Katakana,
                    _ => To.Hiragana
                };
                
                Console.WriteLine("Presentation mode ('1':Normal '2':Spaced '3':Okurigana '4':Furigana Default:Okurigana):");
                Console.Write("> ");
                var modeStr = Console.ReadLine();
                var mode = modeStr switch
                {
                    "1" => Mode.Normal,
                    "2" => Mode.Spaced,
                    "3" => Mode.Okurigana,
                    "4" => Mode.Furigana,
                    _ => Mode.Okurigana
                };

                var system = RomajiSystem.Hepburn;
                if (to == To.Romaji)
                {
                    Console.WriteLine("Romaji system ('1':Nippon '2':Passport '3':Hepburn Default:Hepburn):");
                    Console.Write("> ");
                    var systemStr = Console.ReadLine();
                    system = systemStr switch
                    {
                        "1" => RomajiSystem.Nippon,
                        "2" => RomajiSystem.Passport,
                        "3" => RomajiSystem.Hepburn,
                        _ => RomajiSystem.Hepburn
                    };
                }
                var result = await converter.Convert(str, to, mode, system, "(", ")");
                var pronunciation = new StringBuilder();
                foreach (var div in await converter.GetDivisions(str, to, mode, system, "(", ")"))
                {
                    pronunciation.Append(div.RomaPronunciation);
                }
                Console.WriteLine(result);
                Console.WriteLine($"Pronunciation: {pronunciation}");
                Console.WriteLine();
            }
        }
    }
}