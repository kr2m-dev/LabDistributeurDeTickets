Console.OutputEncoding = System.Text.Encoding.UTF8;

// Dernier numéro attribué par type, stocké dans le dossier temporaire (leçon Path + File)
string cheminFichier = Path.Combine(Path.GetTempPath(), "fnumero.txt");

// Réinitialisation à chaque démarrage : nouvelle journée, plus personne en attente
File.WriteAllText(cheminFichier, "V=0\nR=0\nI=0");

// Lecture des derniers numéros pour chaque type
Dictionary<string, int> dernierNum = LireDerniersNumeros(cheminFichier);

// Liste de tous les clients (ticket, type, compte, nom, prénom) pour l'affichage final
var clients = new List<string[]>();

// Messages créés une seule fois ; on ne change que les valeurs à chaque client
string modeleMessage = "Vous avez le numéro {0}, {1} personne avant vous";
string modelePremier = "Vous avez le numéro {0}, vous êtes le premier de la file";

while (true)
{
    Console.WriteLine("Distributeur de tickets");
    Console.WriteLine("1 - Versement");
    Console.WriteLine("2 - Retrait");
    Console.WriteLine("3 - Informations");
    Console.WriteLine("q - Terminer");
    Console.Write("Quel est votre choix : ");
    string? choix = Console.ReadLine()?.Trim();

    if (choix is null || choix.Equals("q", StringComparison.OrdinalIgnoreCase))
        break;

    string lettre;
    string type;
    if (choix == "1") { lettre = "V"; type = "Versement"; }
    else if (choix == "2") { lettre = "R"; type = "Retrait"; }
    else if (choix == "3") { lettre = "I"; type = "Informations"; }
    else
    {
        Console.WriteLine("Choix invalide. Tape 1, 2, 3 ou q.\n");
        continue;
    }

    try
    {
        string? compte = LireChamp("Numéro de compte");
        if (compte is null) break;

        string? nom = LireChamp("Nom");
        if (nom is null) break;

        string? prenom = LireChamp("Prénom");
        if (prenom is null) break;

        // Incrémenter le compteur du type puis sauvegarder dans le fichier
        dernierNum[lettre]++;
        File.WriteAllText(cheminFichier, Serialiser(dernierNum));

        int rang = dernierNum[lettre];
        string ticket = $"{lettre}-{rang}";

        // Personnes du même type déjà en attente avant ce client
        int attente = rang - 1;

        // Premier de la file : message adapté, sinon nombre de personnes avant
        string message = attente == 0
            ? string.Format(modelePremier, ticket)
            : string.Format(modeleMessage, ticket, attente);
        Console.WriteLine($"\n{message}\n");

        clients.Add(new[] { ticket, type, compte, nom, prenom });
    }
    catch (ArgumentNullException ex)
    {
        Console.WriteLine($"\n[ArgumentNullException] {ex.Message}\n");
        continue;
    }
    catch (IOException ex)
    {
        Console.WriteLine($"\n[IOException] {ex.Message}\n");
        continue;
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine($"\n[UnauthorizedAccessException] {ex.Message}\n");
        continue;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[Exception] {ex.GetType().Name} : {ex.Message}\n");
        continue;
    }

    // Nouveau client ?
    Console.Write("voulez-vous continuer o/n : ");
    string? suite = Console.ReadLine()?.Trim();
    if (suite is null || !suite.Equals("o", StringComparison.OrdinalIgnoreCase))
        break;
}

// Liste de tous les clients et de leur numéro de ticket (fin d'exécution)
Console.WriteLine();
if (clients.Count == 0)
{
    Console.WriteLine("Aucun client enregistré.");
}
else
{
    foreach (string[] c in clients)
        Console.WriteLine($"{c[0]} - {c[4]} {c[3]} - Compte {c[2]} - {c[1]}");
}

// Lit un champ ; renvoie null si l'utilisateur quitte (q ou Ctrl+Z)
// Saisie vide -> ArgumentNullException (message système)
static string? LireChamp(string message)
{
    Console.Write($"{message} (ou 'q' pour quitter) : ");
    string? saisie = Console.ReadLine()?.Trim();

    if (saisie is null || saisie.Equals("q", StringComparison.OrdinalIgnoreCase))
        return null;

    if (string.IsNullOrWhiteSpace(saisie))
        throw new ArgumentNullException(nameof(saisie));

    return saisie;
}

// Lit le fichier : une ligne "Lettre=nombre" par type
static Dictionary<string, int> LireDerniersNumeros(string chemin)
{
    var compteurs = new Dictionary<string, int> { ["V"] = 0, ["R"] = 0, ["I"] = 0 };

    foreach (string ligne in File.ReadAllLines(chemin))
    {
        string[] parties = ligne.Split('=');
        if (parties.Length == 2 &&
            compteurs.ContainsKey(parties[0]) &&
            int.TryParse(parties[1], out int valeur))
        {
            compteurs[parties[0]] = valeur;
        }
    }

    return compteurs;
}

// Sérialise les compteurs pour écriture dans le fichier
static string Serialiser(Dictionary<string, int> compteurs)
    => $"V={compteurs["V"]}\nR={compteurs["R"]}\nI={compteurs["I"]}";
