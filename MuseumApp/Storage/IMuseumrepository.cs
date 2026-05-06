using MuseumApp.Domain;

namespace MuseumApp.Storage
{
    public interface IMuseumRepository
    {
        MuseumData LoadData();
        void SaveData(MuseumData data);
    }
}