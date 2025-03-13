using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using FortniteReplayReader;
using FortniteReplayReader.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

[Route("api/replay")]
[ApiController]
public class ReplayController : ControllerBase
{
    [HttpPost("upload")]
    public IActionResult UploadReplay([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Invalid file.");

        string directoryPath = Path.Combine("Replays");
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        string filePath = Path.Combine(directoryPath, file.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        return Ok(new { FileName = file.FileName });
    }

    [HttpGet("leaderboard")]
    public IActionResult GetLeaderboard([FromQuery] string fileName)
    {
        string filePath = Path.Combine("Replays", Path.GetFileName(fileName));
Console.WriteLine($"Looking for file: {Path.GetFullPath(filePath)}");

if (!System.IO.File.Exists(filePath))
{
    Console.WriteLine("Replay file not found.");
    return NotFound("Replay file not found.");
}
        var replay = new ReplayReader().ReadReplay(filePath);

        var players = replay.PlayerData
            .Where(p => p.Placement != null)
            .OrderBy(p => p.Placement)
            .Select(p => new PlayerData
            {
                Rank = p.Placement ?? 100,
                Player = p.PlayerName ?? "Unknown",
                Eliminations = replay.Eliminations.Count(e => e.Eliminator == p.PlayerId.ToUpper()),
                Score = CalculateScore(p.Placement ?? 100, replay.Eliminations.Count(e => e.Eliminator == p.PlayerId.ToUpper())),
                IsBot = p.IsBot,
                DeathCause = replay.Eliminations.FirstOrDefault(e => e.Eliminated == p.PlayerId)?.IsSelfElimination == true ? "Self-Elimination" : "Eliminated by Player",
                GunType = replay.Eliminations.FirstOrDefault(e => e.Eliminated == p.PlayerId)?.GunType ?? 0,
                Time = replay.Eliminations.FirstOrDefault(e => e.Eliminated == p.PlayerId)?.Time ?? "Unknown"
            }).ToList();

        return Ok(players);
    }

    [HttpGet("download-leaderboard")]
    public IActionResult DownloadLeaderboard([FromQuery] string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return BadRequest("Filename is required.");

        string filePath = Path.Combine("Replays", Path.GetFileName(fileName));

        if (!System.IO.File.Exists(filePath))
            return NotFound("Replay file not found.");

        var replay = new ReplayReader().ReadReplay(filePath);

        var players = replay.PlayerData
            .Where(p => p.Placement != null)
            .OrderBy(p => p.Placement)
            .Select(p => new PlayerData
            {
                Rank = p.Placement ?? 100,
                Player = p.PlayerName ?? "Unknown",
                Eliminations = replay.Eliminations.Count(e => e.Eliminator == p.PlayerId.ToUpper()),
                Score = CalculateScore(p.Placement ?? 100, replay.Eliminations.Count(e => e.Eliminator == p.PlayerId.ToUpper())),
                IsBot = p.IsBot,
                DeathCause = replay.Eliminations.FirstOrDefault(e => e.Eliminated == p.PlayerId)?.IsSelfElimination == true ? "Self-Elimination" : "Eliminated by Player",
                GunType = replay.Eliminations.FirstOrDefault(e => e.Eliminated == p.PlayerId)?.GunType ?? 0,
                Time = replay.Eliminations.FirstOrDefault(e => e.Eliminated == p.PlayerId)?.Time ?? "Unknown"
            }).ToList();

        string excelFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_Leaderboard.xlsx";

        using (var stream = new MemoryStream())
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());

                Sheets sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet()
                {
                    Id = document.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "Leaderboard"
                };
                sheets.Append(sheet);

                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Define headers
                string[] headers = { "Rank", "Player", "Eliminations", "Score", "Is Bot", "Death Cause", "Gun Type", "Time" };
                Row headerRow = new Row();
                foreach (var header in headers)
                {
                    headerRow.Append(new Cell()
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(header)
                    });
                }
                sheetData.Append(headerRow);

                // Populate data
                foreach (var player in players)
                {
                    Row newRow = new Row();
                    newRow.Append(
                        new Cell { DataType = CellValues.Number, CellValue = new CellValue(player.Rank.ToString()) },
                        new Cell { DataType = CellValues.String, CellValue = new CellValue(player.Player) },
                        new Cell { DataType = CellValues.Number, CellValue = new CellValue(player.Eliminations.ToString()) },
                        new Cell { DataType = CellValues.Number, CellValue = new CellValue(player.Score.ToString()) },
                        new Cell { DataType = CellValues.String, CellValue = new CellValue(player.IsBot ? "Yes" : "No") },
                        new Cell { DataType = CellValues.String, CellValue = new CellValue(player.DeathCause) },
                        new Cell { DataType = CellValues.Number, CellValue = new CellValue(player.GunType.ToString()) },
                        new Cell { DataType = CellValues.String, CellValue = new CellValue(player.Time) }
                    );
                    sheetData.Append(newRow);
                }
            }

            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelFileName);
        }
    }

    private int CalculateScore(int placement, int eliminations)
    {
        int score = eliminations * 2;
        return placement switch
        {
            1 => score + 25,
            2 => score + 18,
            3 => score + 15,
            >= 4 and <= 6 => score + 10,
            >= 7 and <= 10 => score + 5,
            >= 11 and <= 15 => score + 3,
            >= 16 and <= 20 => score + 1,
            _ => score
        };
    }
}

// PlayerData class for returning structured leaderboard information
public class PlayerData
{
    public int Rank { get; set; }
    public string Player { get; set; } = "Unknown";
    public int Eliminations { get; set; }
    public int Score { get; set; }
    public bool IsBot { get; set; }
    public string DeathCause { get; set; } = "Unknown";
    public int GunType { get; set; }
    public string Time { get; set; } = "Unknown";
}
