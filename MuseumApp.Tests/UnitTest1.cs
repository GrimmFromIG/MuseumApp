using Xunit;
using Moq;
using System;
using System.IO;
using System.Collections.Generic;
using MuseumApp.Domain;
using MuseumApp.Services;
using MuseumApp.Storage;

namespace MuseumApp.Tests
{
    public class MuseumTests
    {
        // ==========================================================
        // БІЗНЕС-ЛОГІКА (з використанням Mocks для ізоляції)
        // ==========================================================

        [Fact]
        public void AddExhibit_DuplicateInventoryNumber_ThrowsException()
        {
            // Тест-кейс: CL 1.2 (Негативний сценарій)
            // Arrange (Підготовка)
            var hallId = Guid.NewGuid();
            var mockRepo = new Mock<IMuseumRepository>(); // Створюємо заглушку бази даних
            
            // Налаштовуємо заглушку: вона поверне базу, де вже є експонат "ART-001"
            var fakeData = new MuseumData {
                Halls = new List<ExhibitionHall> {
                    new ExhibitionHall { 
                        Id = hallId, 
                        Items = new List<Exhibit> { new Exhibit { InventoryNumber = "ART-001" } } 
                    }
                }
            };
            mockRepo.Setup(r => r.LoadData()).Returns(fakeData);
            
            var service = new MuseumManagerService(mockRepo.Object);
            var duplicateExhibit = new Exhibit { InventoryNumber = "ART-001", Title = "Картина 2" };

            // Act & Assert (Дія та Перевірка негативного сценарію)[cite: 5]
            var exception = Assert.Throws<Exception>(() => service.AddExhibit(hallId, duplicateExhibit));
            Assert.Contains("вже існує", exception.Message);
        }

        [Fact]
        public void MoveExhibit_ToDisplayHall_ChangesStatusToOnDisplay()
        {
            // Тест-кейс: CL 1.3 (Позитивний сценарій)[cite: 5]
            // Arrange
            var sourceHall = new ExhibitionHall { Id = Guid.NewGuid(), Name = "Сховище" };
            var targetHall = new ExhibitionHall { Id = Guid.NewGuid(), Name = "Виставковий Зал" };
            var exhibit = new Exhibit { InventoryNumber = "ART-002", Status = ConditionStatus.InStorage };
            sourceHall.Items.Add(exhibit);

            var mockRepo = new Mock<IMuseumRepository>();
            mockRepo.Setup(r => r.LoadData()).Returns(new MuseumData { Halls = new List<ExhibitionHall> { sourceHall, targetHall } });
            
            var service = new MuseumManagerService(mockRepo.Object);

            // Act (Дія)
            service.MoveExhibit("ART-002", targetHall.Id);

            // Assert (Перевірка позитивного результату)[cite: 5]
            Assert.Equal(ConditionStatus.OnDisplay, exhibit.Status); // Статус мав змінитись
            Assert.Contains(exhibit, targetHall.Items);              // Експонат має бути в новому залі
        }

        // ==========================================================
        // ДОПОМІЖНІ ЗАВДАННЯ (Робота з файловою системою)
        // ==========================================================

        [Fact]
        public void LoadData_FileDoesNotExist_ReturnsEmptyMuseumData()
        {
            // Тест-кейс: CL 2.1 (Позитивний/Межовий сценарій)[cite: 5]
            // Arrange
            string fakePath = "non_existent_file.json";
            if (File.Exists(fakePath)) File.Delete(fakePath); // Гарантуємо відсутність
            var repo = new JsonMuseumRepository(fakePath);

            // Act
            var result = repo.LoadData();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Halls);
            Assert.Empty(result.Halls); // Повинна повернутись порожня база
        }

        [Fact]
        public void SaveData_ValidData_CreatesJsonFile()
        {
            // Тест-кейс: CL 2.2 (Позитивний сценарій)[cite: 5]
            // Arrange
            string tempPath = "temp_test_data.json";
            var repo = new JsonMuseumRepository(tempPath);
            var dataToSave = new MuseumData {
                Halls = new List<ExhibitionHall> { new ExhibitionHall { Name = "Test Hall" } }
            };

            // Act
            repo.SaveData(dataToSave);

            // Assert
            Assert.True(File.Exists(tempPath));
            string jsonContent = File.ReadAllText(tempPath);
            Assert.Contains("Test Hall", jsonContent);

            // Cleanup (Прибираємо за собою файл)
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}
