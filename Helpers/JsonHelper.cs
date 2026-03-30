using System;
using System.IO;
using System.Text.Json;

namespace NovaniX_EM2.Helpers
{
    /// <summary>
    /// 공용으로 사용하는 JSON 파일 관리 헬퍼 클래스
    /// </summary>
    public static class JsonHelper
    {
        // JSON 직렬화 옵션 (보기 좋게 들여쓰기, 대소문자 무시 등)
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// JSON 파일 생성 및 저장 (기존 파일이 있으면 덮어씁니다)
        /// </summary>
        public static void Save<T>(string filePath, T data)
        {
            try
            {
                // 디렉토리가 없으면 생성 (string? 로 변경하여 CS8600 경고 해결)
                string? directory = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string jsonString = JsonSerializer.Serialize(data, _options);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON 저장 오류: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// JSON 파일 읽기 (파일이 없거나 비어있으면 null 반환 가능하므로 T? 사용)
        /// </summary>
        public static T? Load<T>(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return default;
                }

                string jsonString = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(jsonString, _options);
            }
            catch (JsonException ex)
            {
                // 기존 데이터 포맷과 다르거나 파일이 손상된 경우 프로그램 크래시 방지
                System.Diagnostics.Debug.WriteLine($"JSON 파싱 오류 (구조 불일치): {ex.Message}");
                return default; // null을 반환하여 ViewModel에서 새 구조로 덮어쓰도록 유도
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON 읽기 오류: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// JSON 파일 수정 (기존 데이터를 읽어와 콜백으로 수정한 뒤 다시 저장)
        /// </summary>
        public static void Update<T>(string filePath, Action<T> updateAction) where T : class, new()
        {
            try
            {
                // 기존 데이터 로드, 없으면 새 인스턴스 생성
                T data = Load<T>(filePath) ?? new T();

                // 데이터 수정 로직 실행
                updateAction(data);

                // 다시 저장
                Save(filePath, data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON 수정 오류: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// JSON 파일 삭제
        /// </summary>
        public static void Delete(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"JSON 삭제 오류: {ex.Message}");
                throw;
            }
        }
    }
}