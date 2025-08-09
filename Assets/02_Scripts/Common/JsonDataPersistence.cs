using System;
using System.IO;
using UnityEngine;

namespace _02_Scripts.Common
{
    /// <summary>
    /// Simple JSON Persistence
    /// - Auto create directory (자동 디렉터리 생성 / auto create directory)
    /// - Pretty print option (가독성 출력 / pretty print)
    /// - Try/catch logging only (간단 예외 처리 / simple exception handling)
    /// </summary>
    public static class JsonStorage
    {
        /// <summary>
        /// Save object as JSON to file. (객체를 JSON으로 저장)
        /// </summary>
        public static bool Save<T>(T data, string filePath, bool prettyPrint = true)
        {
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonUtility.ToJson(data, prettyPrint);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonStorage] Save failed ({filePath}): {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load JSON from file into object. If missing or invalid, return defaultFactory/new T.
        /// (파일에서 JSON 로드. 없거나 깨지면 기본값 반환)
        /// </summary>
        public static T LoadOrDefault<T>(string filePath, Func<T> defaultFactory = null) where T : new()
        {
            try
            {
                if (!File.Exists(filePath))
                    return defaultFactory != null
                        ? defaultFactory()
                        : new T();

                var json = File.ReadAllText(filePath);
                var obj = JsonUtility.FromJson<T>(json);

                // FromJson can return null for classes. (클래스일 때 null 가능)
                return obj != null
                    ? obj
                    : (defaultFactory != null
                        ? defaultFactory()
                        : new T());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonStorage] Load failed ({filePath}): {ex.Message}");
                return defaultFactory != null
                    ? defaultFactory()
                    : new T();
            }
        }

        /// <summary>Check existence. (파일 존재 확인)</summary>
        public static bool Exists(string filePath) => File.Exists(filePath);

        /// <summary>Delete file. (파일 삭제)</summary>
        public static bool Delete(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return false;
                File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonStorage] Delete failed ({filePath}): {ex.Message}");
                return false;
            }
        }

        /// <summary>Build save path under persistentDataPath. (저장 경로 생성)</summary>
        public static string GetSavePath(string fileName)
            => Path.Combine(Application.persistentDataPath, fileName);
    }
}