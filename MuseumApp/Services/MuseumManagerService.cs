using System;
using System.Collections.Generic;
using System.Linq;
using MuseumApp.Domain;
using MuseumApp.Storage;

namespace MuseumApp.Services
{
    public class MuseumManagerService
    {
        private readonly IMuseumRepository _repository;
        private MuseumData _data;

        public MuseumManagerService(IMuseumRepository repository)
        {
            _repository = repository;
            _data = _repository.LoadData();
        }

        public List<ExhibitionHall> GetAllHalls() => _data.Halls;

        public void AddExhibit(Guid hallId, Exhibit exhibit)
        {
            if (_data.Halls.Any(h => h.Items.Any(i => i.InventoryNumber == exhibit.InventoryNumber)))
                throw new Exception("Экспонат с таким номером уже существует.");

            var hall = _data.Halls.FirstOrDefault(h => h.Id == hallId);
            if (hall != null)
            {
                hall.Items.Add(exhibit);
                _repository.SaveData(_data);
            }
        }

        public void MoveExhibit(string inventoryNum, Guid targetHallId)
        {
            Exhibit? exhibit = null;
            ExhibitionHall? currentHall = null;

            foreach (var h in _data.Halls)
            {
                exhibit = h.Items.FirstOrDefault(e => e.InventoryNumber == inventoryNum);
                if (exhibit != null) { currentHall = h; break; }
            }

            if (exhibit == null || currentHall == null) throw new Exception("Экспонат не найден.");

            var targetHall = _data.Halls.FirstOrDefault(h => h.Id == targetHallId);
            if (targetHall == null) throw new Exception("Целевой зал не найден.");

            currentHall.Items.Remove(exhibit);
            
            exhibit.Status = targetHall.Name.Contains("сховище", StringComparison.OrdinalIgnoreCase) 
                ? ConditionStatus.InStorage 
                : ConditionStatus.OnDisplay;

            targetHall.Items.Add(exhibit);
            _repository.SaveData(_data);
        }

        public void AddHall(string name)
        {
            if (_data.Halls.Any(h => h.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("Зал уже существует.");

            _data.Halls.Add(new ExhibitionHall { Name = name });
            _repository.SaveData(_data);
        }

        public List<Exhibit> FindExhibits(string? text, Category? category)
        {
            var all = _data.Halls.SelectMany(h => h.Items);
            if (!string.IsNullOrEmpty(text))
                all = all.Where(e => e.Title.Contains(text, StringComparison.OrdinalIgnoreCase) || 
                                     e.InventoryNumber.Contains(text));
            if (category.HasValue)
                all = all.Where(e => e.Category == category.Value);
            return all.ToList();
        }

        public void RemoveExhibit(string invNum)
        {
            foreach (var hall in _data.Halls)
            {
                var item = hall.Items.FirstOrDefault(i => i.InventoryNumber == invNum);
                if (item != null)
                {
                    hall.Items.Remove(item);
                    _repository.SaveData(_data);
                    return;
                }
            }
            throw new Exception("Экспонат не найден.");
        }

        public void RemoveHall(Guid hallId)
    {
        var hall = _data.Halls.FirstOrDefault(h => h.Id == hallId);
        if (hall == null) throw new Exception("Зал не знайдено.");

        // Захист: забороняємо видаляти зал, якщо він не порожній
        if (hall.Items.Any())
        {
            throw new Exception("Не можна видалити зал із експонатами. Спочатку перемістіть їх.");
        }

        _data.Halls.Remove(hall);
        _repository.SaveData(_data);
    }
    }
}