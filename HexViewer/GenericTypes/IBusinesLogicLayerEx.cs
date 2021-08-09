using Cryptosoft.TypesGallery.MVPVM;


/// <summary>
///     Не используется. Оставил на всякий случай.
/// </summary>
namespace HexViewer.GenericTypes
{
    public interface IBusinesLogicLayerEx : IBusinesLogicLayer
    {
        // Массив полученных байтов с проецирования.
        byte[] MMFBytes { get; set; }

        // Смещение проецирования.
        long StartOffset { get; set; }

        // Длина чтения byte из MMF.
        int RangeRead { get; set; }

        // Задание пути к файлу, передача View размера файла.
        long SetFile(string filePath);

        // Чтение спроецированного файла; работа с Accessor'ами.
        void ReadFile();
    }
}