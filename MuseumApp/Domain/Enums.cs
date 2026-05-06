namespace MuseumApp.Domain
{
    public enum Category
    {
        Painting,   // Живопись
        Sculpture,  // Скульптура
        Artifact,   // Артефакт
        Document    // Документ
    }

    public enum ConditionStatus
    {
        OnDisplay,        // В экспозиции
        InStorage,        // В хранилище
        UnderRestoration, // На реставрации
        LoanedOut,        // Выдано (другому музею)
        WrittenOff        // Списано
    }
}