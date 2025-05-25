using System;
using System.Collections.Generic;
using System.Security.Cryptography; 
using System.Text;                  
using System.Linq;                
using System.Diagnostics;          
using System.Globalization;         

// Definición de la clase Node (Nodo)
public class Node
{
    public string Partida { get; set; }
    public List<int> Cuerpo { get; set; } 
    public string FirmaDigital { get; set; }
    public Node Next { get; set; } 

    public Node(string partida, List<int> cuerpo, string firmaDigital)
    {
        Partida = partida;
        Cuerpo = cuerpo;
        FirmaDigital = firmaDigital;
        Next = null; 
    }

    // Sobrescribimos ToString() para facilitar la impresión y depuración
    public override string ToString()
    {
        return $"Partida: {Partida}\n" +
               $"Cuerpo: [{string.Join(", ", Cuerpo)}]\n" + // string.Join para formatear la lista
               $"Firma Digital: {FirmaDigital}\n";
    }
}

public static class AutomataBuilder
{
    // Generador de números aleatorios para usar en toda la clase
    private static readonly Random _random = new Random();

    /// <param name="dataString">La cadena de texto a hashear.</param>
    /// <returns>El hash SHA256 en formato hexadecimal.</returns>
    public static string GenerateSha256Hash(string dataString)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            // ComputeHash - calcula el hash de la cadena de entrada
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(dataString));

            // StringBuilder para recolectar los bytes y crear una cadena
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2")); // "x2" formatea como hexadecimal de dos dígitos
            }
            return builder.ToString();
        }
    }

 
    /// <param name="n">Número de nodos a crear.</param>
    /// <param name="k">Número de elementos aleatorios en el cuerpo de cada nodo.</param>
    /// <returns>La cabeza de la lista enlazada, o null si n es 0.</returns>
    public static Node BuildLinkedList(int n, int k)
    {
        Node head = null;
        Node currentNode = null;
        string previousNodeSignature = null;

        for (int i = 0; i < n; i++)
        {
            // 1. Generar cuerpo: k elementos enteros aleatorios entre 1 y 100.000
            List<int> bodyElements = new List<int>(k);
            for (int j = 0; j < k; j++)
            {
                bodyElements.Add(_random.Next(1, 100001)); // _random.Next(min, max_exclusivo)
            }
            // Convertir los elementos del cuerpo a una cadena separada por espacios para el hash
            string bodyString = string.Join(" ", bodyElements);

            // 2. Determinar el valor de 'partida'
            string partidaValue;
            if (i == 0)
            {
                // Primer nodo: la 'partida' es el SHA256 de la fecha y hora actual
                string currentDateTimeStr = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                partidaValue = GenerateSha256Hash(currentDateTimeStr);
            }
            else
            {
                // Para nodos subsiguientes: la 'partida' es la firma digital del nodo anterior
                partidaValue = previousNodeSignature;
            }

            // 3. Calcular la 'firma_digital' del nodo actual
            string contentToHash = $"{partidaValue} {bodyString}";
            string currentNodeSignature = GenerateSha256Hash(contentToHash);

            // 4. Crear una nueva instancia de Node
            Node newNode = new Node(partidaValue, bodyElements, currentNodeSignature);

            // 5. Enlazar el nuevo nodo a la lista
            if (head == null)
            {
                head = newNode;
            }
            else
            {
                currentNode.Next = newNode;
            }

            // El nuevo nodo se convierte en el nodo actual para la próxima iteración
            currentNode = newNode;
            // Almacenar la firma del nodo actual para que sea la 'partida' del siguiente
            previousNodeSignature = currentNodeSignature;
        }

        return head;
    }

    /// <param name="head">La cabeza de la lista a liberar.</param>
    public static void DisposeLinkedList(Node head)
    {
        Node current = head;
        while (current != null)
        {
            Node next = current.Next;
            // Opcional: limpiar referencias explícitamente para ayudar al GC si el objeto es muy grande
            current.Cuerpo = null;
            current.Next = null;
            current = next;
        }
    }


    /// <param name="n">Número de nodos.</param>
    /// <param name="k">Número de elementos en el cuerpo de cada nodo.</param>
    /// <returns>Tiempo de ejecución en milisegundos.</returns>
    public static double RunScenario(int n, int k)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start(); // Inicia el temporizador

        Node linkedListHead = BuildLinkedList(n, k); // Construye la lista

        stopwatch.Stop(); // Detiene el temporizador


        DisposeLinkedList(linkedListHead);

        return stopwatch.Elapsed.TotalMilliseconds; // Tiempo transcurrido en milisegundos
    }
}


class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Ejecutando escenarios en C# ---\n");

        // Definición de los escenarios de prueba
        var scenarios = new List<(int n, int k)>
        {
            (3, 4),
            (10, 200),
            (200, 10)
        };

        var csharpResults = new Dictionary<string, double>(); 

        foreach (var scenario in scenarios)
        {
            int n = scenario.n;
            int k = scenario.k;
            Console.WriteLine($"\nEscenario: n={n}, k={k}");

            // Ejecutar cada escenario varias veces para obtener un promedio más preciso
            int numRuns = 5; 
            double totalTime = 0;
            for (int run = 0; run < numRuns; run++)
            {
                totalTime += AutomataBuilder.RunScenario(n, k);
            }
           // Calcula el tiempo promedio
            double avgTime = totalTime / numRuns; 
            csharpResults[$"n={n}, k={k}"] = avgTime; 
            Console.WriteLine($"Tiempo promedio: {avgTime:F4} ms"); 
        }

        Console.WriteLine("\n--- Resultados Finales de C# ---");
        foreach (var entry in csharpResults)
        {
            Console.WriteLine($"{entry.Key}: {entry.Value:F4} ms");
        }
    }
}
