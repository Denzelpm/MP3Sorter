using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP3Sorter
{
	internal class Program
	{
		private static string screenshotsFolderPath; // Folder to store the screenshots
		private static int screenshotCount = 0; // Counter for the screenshots

		static void Main(string[] args)
		{
			Console.WriteLine("Introduzca la ruta a la carpeta de origen: ");
			string sourceFolderPath = Console.ReadLine();
			Console.WriteLine();

			Console.WriteLine("Introduzca la ruta de la carpeta de destino: ");
			string destinationFolderPath = Console.ReadLine();
			Console.WriteLine();

			Console.WriteLine("Introduzca la ruta de la carpeta para guardar las capturas de pantalla:");
			screenshotsFolderPath = Console.ReadLine();
			Console.WriteLine();

			Console.WriteLine("Elija cómo organizar los archivos:");
			Console.WriteLine("1. Por Año");
			Console.WriteLine("2. Por Nombre del Álbum");
			string organizationOption = Console.ReadLine();
			Console.WriteLine();


			string[] mp3Files = Directory.GetFiles(sourceFolderPath, "*.mp3");

			var stopWatch = Stopwatch.StartNew();

			Parallel.ForEach(mp3Files, mp3File =>
			{
				try
				{
					// Get the organization property based on user input
					string organizationProperty = GetOrganizationProperty(mp3File, organizationOption);

					// Set the destination folder
					string destinationPropertyFolderPath = SetDestinationFolder(destinationFolderPath, organizationProperty);

					// Copy the file to the destination folder
					CopyFile(mp3File, destinationPropertyFolderPath);

					Console.WriteLine($"Se ha copiado [{Path.GetFileName(mp3File)}] a la carpeta [{organizationProperty}].");

					// Capture a screenshot every second
					CaptureScreenshot();

				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error al copiar {mp3File}: {ex.Message}");
				}
			});

			Console.WriteLine();
			Console.WriteLine("Proceso completado en {0:F2} segundos\n", stopWatch.Elapsed.TotalSeconds);
			Console.WriteLine("Presiona cualquier tecla para salir");
			Console.ReadKey();
		}

		private static void CopyFile(string mp3File, string destinationPropertyFolderPath)
		{
			string destinationFilePath = Path.Combine(destinationPropertyFolderPath, Path.GetFileName(mp3File));

			// Copy the file to the corresponding property folder
			File.Copy(mp3File, destinationFilePath, true);
		}

		private static string SetDestinationFolder(string destinationFolderPath, string organizationProperty)
		{
			string destinationPropertyFolderPath = Path.Combine(destinationFolderPath, organizationProperty);

			// Create the property folder if it doesn't exist
			if (!Directory.Exists(destinationPropertyFolderPath))
			{
				Directory.CreateDirectory(destinationPropertyFolderPath);
			}

			return destinationPropertyFolderPath;
		}

		private static string GetOrganizationProperty(string mp3File, string organizationOption)
		{
			var file = TagLib.File.Create(mp3File);
			string organizationProperty;

			switch (organizationOption)
			{
				case "1":
					organizationProperty = file.Tag.Year.ToString();
					break;
				case "2":
					organizationProperty = file.Tag.Album ?? "Unknown Album";
					break;
				default:
					organizationProperty = file.Tag.Year.ToString();
					break;
			}

			return organizationProperty;
		}

		private static void CaptureScreenshot()
		{
			string screenshotPath;
			lock (screenshotsFolderPath)
			{
				screenshotPath = Path.Combine(screenshotsFolderPath, $"Screenshot_{screenshotCount}.png");
				screenshotCount++;
			}

			using (Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
			{
				using (Graphics graphics = Graphics.FromImage(bitmap))
				{
					graphics.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, bitmap.Size);
				}

				using (MemoryStream memoryStream = new MemoryStream())
				{
					bitmap.Save(memoryStream, ImageFormat.Png);
					byte[] imageBytes = memoryStream.ToArray();
					File.WriteAllBytes(screenshotPath, imageBytes);
				}
			}
		}
	}
}
