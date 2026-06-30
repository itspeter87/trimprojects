using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TRIM.SDK;

namespace audioextractor
{
    public class EventProcessor : TrimEventProcessorAddIn

    {
        public override void ProcessEvent(Database db, TrimEvent evt)
        {
            var rec = (Record)db.FindTrimObjectByUri(BaseObjectTypes.Record, evt.ObjectUri);
            var recExt = rec.Extension?.Trim().ToLower() ?? "";

            Log("Processing record: " + rec.Number + " (" + recExt + ")");

            var allowedExtensions = new List<string> { "mp3", "wav" };

            if (allowedExtensions.Contains(recExt))
            {
                Log("Extension allowed. Preparing transcription.");

                rec.LoadDocumentIntoClientCache();

                var localPath = rec.GetDocumentPathInClientCache(Events.DocExtracted);
                Log("Local cached path: " + localPath);

                var txtPath = TranscribeAudio(localPath);
                Log("Whisper output: " + txtPath);

                rec.ChildRenditions.NewRendition(txtPath, RenditionType.Ocr, "Extracted Audio");
                rec.Save();

                Log("OCR rendition saved for record " + rec.Number);
            }
            else
            {
                Log("Extension not allowed. Skipping.");
            }
        }

        private string TranscribeAudio(string audioPath)
        {
            var outputDir = @"C:\whisper\process";
            Directory.CreateDirectory(outputDir);

            var modelPath = @"C:\Whisper\models\ggml-small-q8_0.bin";
            var whisperExe = @"C:\Whisper\whisper-cli.exe";

            // Whisper.cpp requires output-file WITHOUT extension
            var outputBase = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(audioPath));

            var psi = new ProcessStartInfo
            {
                FileName = whisperExe,
                Arguments = $"--model \"{modelPath}\" -otxt --output-file \"{outputBase}\" \"{audioPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var p = Process.Start(psi))
            {
                p.WaitForExit();
            }

            var txtPath = outputBase + ".txt";
            return File.Exists(txtPath) ? txtPath : null;
        }


        private void Log(string message)
        {
            var logPath = @"C:\temp\audioextractor.log";

            Directory.CreateDirectory(Path.GetDirectoryName(logPath));

            var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + message;

            File.AppendAllText(logPath, line + Environment.NewLine);
        }

    }

}


