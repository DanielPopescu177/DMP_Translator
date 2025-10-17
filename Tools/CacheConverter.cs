using System;
using System.IO;
using System.Collections.Generic;

namespace DMPTranslator.Tools
{
    class CacheConverter
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("===================================");
            Console.WriteLine("DMP Translator 캐시 변환 툴");
            Console.WriteLine("===================================");
            Console.WriteLine();

            // 캐시 파일 경로 입력
            string inputPath = "";
            if (args.Length > 0)
            {
                inputPath = args[0];
            }
            else
            {
                Console.WriteLine("변환할 캐시 파일 경로를 입력하세요:");
                Console.WriteLine("(또는 파일을 드래그 앤 드롭하세요)");
                inputPath = Console.ReadLine()?.Trim('"');
            }

            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
            {
                Console.WriteLine();
                Console.WriteLine("❌ 파일을 찾을 수 없습니다!");
                Console.WriteLine();
                Console.WriteLine("아무 키나 눌러 종료...");
                Console.ReadKey();
                return;
            }

            try
            {
                // 백업 생성
                string backupPath = inputPath + ".backup";
                File.Copy(inputPath, backupPath, true);
                Console.WriteLine($"✅ 백업 생성: {Path.GetFileName(backupPath)}");
                Console.WriteLine();

                // 캐시 읽기
                var cache = new Dictionary<string, string>();
                var lines = File.ReadAllLines(inputPath);
                int converted = 0;
                int errors = 0;

                Console.WriteLine("변환 중...");
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        string original = "";
                        string translated = "";

                        // 1. 탭 구분자 (기존 방식 1)
                        if (line.Contains("\t"))
                        {
                            var parts = line.Split(new[] { '\t' }, 2);
                            if (parts.Length == 2)
                            {
                                original = parts[0];
                                translated = parts[1];
                            }
                        }
                        // 2. = 구분자 (기존 방식 2)
                        else if (line.Contains("=") && !line.Contains("==>"))
                        {
                            var index = line.IndexOf('=');
                            if (index > 0)
                            {
                                original = line.Substring(0, index);
                                translated = line.Substring(index + 1);
                            }
                        }
                        // 3. 이미 ==> 형식
                        else if (line.Contains("==>"))
                        {
                            var index = line.IndexOf("==>");
                            if (index > 0)
                            {
                                original = line.Substring(0, index);
                                translated = line.Substring(index + 3);
                                // \\n 복원
                                original = original.Replace("\\n", "\n");
                                translated = translated.Replace("\\n", "\n");
                            }
                        }

                        if (!string.IsNullOrEmpty(original) && !string.IsNullOrEmpty(translated))
                        {
                            cache[original] = translated;
                            converted++;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"⚠️ 변환 실패: {line.Substring(0, Math.Min(50, line.Length))}...");
                        errors++;
                    }
                }

                // 새 형식으로 저장
                var newLines = new List<string>();
                foreach (var kvp in cache)
                {
                    // 줄바꿈을 \n으로 변환
                    var key = kvp.Key.Replace("\n", "\\n").Replace("\r", "");
                    var value = kvp.Value.Replace("\n", "\\n").Replace("\r", "");
                    newLines.Add($"{key}==>{value}");
                }

                File.WriteAllLines(inputPath, newLines, System.Text.Encoding.UTF8);

                Console.WriteLine();
                Console.WriteLine("===================================");
                Console.WriteLine("✅ 변환 완료!");
                Console.WriteLine("===================================");
                Console.WriteLine($"총 항목: {converted}개");
                Console.WriteLine($"오류: {errors}개");
                Console.WriteLine();
                Console.WriteLine($"원본: {Path.GetFileName(inputPath)}");
                Console.WriteLine($"백업: {Path.GetFileName(backupPath)}");
                Console.WriteLine();
                Console.WriteLine("변환된 형식: 원문==>번역문");
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine($"❌ 오류 발생: {e.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("아무 키나 눌러 종료...");
            Console.ReadKey();
        }
    }
}
