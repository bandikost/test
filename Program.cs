using System;
using System.Collections.Generic;
using System.Linq;

public abstract class StorageItem
{
    public string ID { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double Depth { get; set; }
    public double Weight { get; set; }

    public double Volume => Width * Height * Depth;

    public StorageItem(string id, double width, double height, double depth, double weight)
    {
        ID = id;
        Width = width;
        Height = height;
        Depth = depth;
        Weight = weight;
    }
}

public class Box : StorageItem
{
    public DateTime? ExpirationDate { get; set; } 
    public DateTime? ProductionDate { get; set; }

    public Box(string id, double width, double height, double depth, double weight, DateTime? expirationDate = null, DateTime? productionDate = null)
        : base(id, width, height, depth, weight)
    {
        ExpirationDate = expirationDate;
        ProductionDate = productionDate;

        if (ProductionDate.HasValue && !ExpirationDate.HasValue)
        {
            ExpirationDate = ProductionDate.Value.AddDays(100);
        }
    }
}

public class Pallet : StorageItem
{
    public List<Box> Boxes { get; private set; } = [];

    public Pallet(string id, double width, double height, double depth)
        : base(id, width, height, depth, 30) // Паллет весит 30кг
    { }

    public void AddBox(Box box)
    {
        if (box.Width > Width || box.Depth > Depth)
        {
            throw new ArgumentException("Коробка не должна превышать размеры паллеты по ширине и глубине.");
        }

        Boxes.Add(box);
        Weight += box.Weight; 
    }

    public DateTime? ExpirationDate
    {
        get
        {
            if (Boxes.Count == 0) return null;
            return Boxes.Min(box => box.ExpirationDate);
        }
    }

    public new double Volume
    {
        get
        {
            double boxesVolume = Boxes.Sum(box => box.Volume);
            return base.Volume + boxesVolume;
        }
    }
}

public class Program
{
    static void Main()
    {
      
        List<Pallet> pallets = GenerateData();

        var groupedPallets = pallets
            .Where(p => p.ExpirationDate.HasValue)
            .OrderBy(p => p.ExpirationDate)
            .ThenBy(p => p.Weight)
            .GroupBy(p => p.ExpirationDate);

        Console.WriteLine("Паллеты, сгруппированные по сроку годности:");
        foreach (var group in groupedPallets)
        {
            Console.WriteLine($"Срок годности: {group.Key.Value.ToShortDateString()}");
            foreach (var pallet in group)
            {
                Console.WriteLine($"  Паллет ID: {pallet.ID}, Вес: {pallet.Weight} кг, Объем: {pallet.Volume} м³");
            }
        }

        var topPallets = pallets
            .Where(p => p.ExpirationDate.HasValue)
            .OrderByDescending(p => p.ExpirationDate)
            .Take(3)
            .OrderBy(p => p.Volume);

        Console.WriteLine("\nТоп 3 паллеты с наибольшим сроком годности, отсортированные по объему:");
        foreach (var pallet in topPallets)
        {
            Console.WriteLine($"Паллет ID: {pallet.ID}, Срок годности: {pallet.ExpirationDate.Value.ToShortDateString()}, Объем: {pallet.Volume} м³");
        }
    }

    static List<Pallet> GenerateData()
    {
        var pallet1 = new Pallet("Pallet1", 1.2, 1.5, 1.0);
        pallet1.AddBox(new Box("Box1", 0.5, 0.5, 0.5, 10, expirationDate: new DateTime(2024, 12, 1)));
        pallet1.AddBox(new Box("Box2", 0.6, 0.4, 0.5, 8, productionDate: new DateTime(2024, 9, 1)));

        var pallet2 = new Pallet("Pallet2", 1.5, 1.2, 1.0);
        pallet2.AddBox(new Box("Box3", 0.7, 0.6, 0.5, 12, productionDate: new DateTime(2024, 8, 15)));
        pallet2.AddBox(new Box("Box4", 0.4, 0.3, 0.5, 5, expirationDate: new DateTime(2024, 10, 15)));

        var pallet3 = new Pallet("Pallet3", 1.3, 1.2, 1.1);
        pallet3.AddBox(new Box("Box5", 0.6, 0.6, 0.6, 15, productionDate: new DateTime(2024, 7, 1)));

        return [ pallet1, pallet2, pallet3 ];
    }
}