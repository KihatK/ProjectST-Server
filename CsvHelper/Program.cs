using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class Program {
    public static void Main(string[] args) {
        string csvFilePath = "../../../../Server/Data/ItemData.csv"; // CSV 파일 경로 입력
        string jsonOutputPath = "../../../../Server/Data/ItemData.json"; // 출력할 JSON 파일 경로

        // CSV 파일의 모든 라인 읽기
        var csvLines = File.ReadAllLines(csvFilePath);

        // CSV의 첫 번째 줄은 헤더이므로 분리
        var headers = csvLines[0].Split(',');

        // 데이터를 저장할 리스트 생성
        var records = new List<Dictionary<string, string>>();

        // 각 데이터 라인을 읽어 딕셔너리로 변환
        for (int i = 1; i < csvLines.Length; i++) {
            var values = csvLines[i].Split(',');
            var record = new Dictionary<string, string>();

            for (int j = 0; j < headers.Length; j++) {
                record[headers[j]] = values[j];
            }

            records.Add(record);
        }

        // 리스트를 JSON 형식으로 변환
        string json = JsonConvert.SerializeObject(records, Formatting.Indented);
        File.WriteAllText(jsonOutputPath, json);

        Console.WriteLine("CSV 파일이 JSON으로 성공적으로 변환되었습니다.");
    }
}