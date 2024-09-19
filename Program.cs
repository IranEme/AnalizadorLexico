// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Collections.Generic;

// Clase que define un Token con su tipo y contenido
public class Token
{
    public string Categoria { get; set; }
    public string Contenido { get; set; }

    public Token(string categoria, string contenido)
    {
        Categoria = categoria;
        Contenido = contenido;
    }

    public override string ToString()
    {
        return $"[Categoria: {Categoria}, Contenido: {Contenido}]";
    }
}

// Clase que representa el Analizador Léxico (Lexer)
public class Lexer
{
    private readonly string _codigoFuente;
    private int _indiceActual;

    // Constructor 
    public Lexer(string rutaArchivo)
    {
        _codigoFuente = File.ReadAllText(rutaArchivo);
        _indiceActual = 0;
    }

    // Método principal para realizar el análisis léxico del código fuente
    public List<Token> Analizar()
    {
        List<Token> tokens = new List<Token>();

        while (_indiceActual < _codigoFuente.Length)
        {
            char caracterActual = _codigoFuente[_indiceActual];

            if (char.IsWhiteSpace(caracterActual))
            {
                _indiceActual++; // Ignorar los espacios
            }
            else if (char.IsLetter(caracterActual))
            {
                tokens.Add(LeerIdentificadorOClave());
            }
            else if (char.IsDigit(caracterActual))
            {
                tokens.Add(LeerNumero());
            }
            else if (caracterActual == '/' && Siguiente() == '/')
            {
                if (Peek() == '/')
                {
                    tokens.Add(LeerComentarioDoble());
                }
                else
                {
                    tokens.Add(LeerComentarioLinea());
                }
            }
            else if (caracterActual == '/' && Siguiente() == '*')
            {
                tokens.Add(LeerComentarioMultilinea());
            }
            else if (caracterActual == '"')
            {
                tokens.Add(LeerCadenaTexto());
            }
            else if (caracterActual == '\'')
            {
                tokens.Add(LeerCaracter());
            }
            else
            {
                tokens.Add(LeerDelimitadorOOperador());
            }
        }

        return tokens;
    }

    // Método para leer identificadores y palabras clave
    private Token LeerIdentificadorOClave()
    {
        string identificador = "";
        while (_indiceActual < _codigoFuente.Length && (char.IsLetterOrDigit(_codigoFuente[_indiceActual]) || _codigoFuente[_indiceActual] == '_'))
        {
            identificador += _codigoFuente[_indiceActual];
            _indiceActual++;
        }

        // Verificación de palabras clave
         string[] palabrasClave = { "int", "float", "if", "else", "while", "return", "true", "false", "null", 
                               "for", "switch", "case", "break", "continue", "void", "double", "char", 
                               "do", "sizeof" };
        if (Array.Exists(palabrasClave, clave => clave == identificador))
        {
            return new Token("PalabraClave", identificador);
        }

        return new Token("Identificador", identificador);
    }

    // Método para leer números, tanto enteros como decimales
    private Token LeerNumero()
    {
        string valorNumerico = "";
        bool esDecimal = false; // Bandera para detectar si es decimal

        while (_indiceActual < _codigoFuente.Length && (char.IsDigit(_codigoFuente[_indiceActual]) || _codigoFuente[_indiceActual] == '.'))
        {
            if (_codigoFuente[_indiceActual] == '.')
            {
                esDecimal = true; // Si contiene un punto decimal, es decimal
            }
            valorNumerico += _codigoFuente[_indiceActual];
            _indiceActual++;
        }

        if (esDecimal)
        {
            return new Token("Decimal", valorNumerico);
        }
        else
        {
            return new Token("Entero", valorNumerico);
        }
    }

    // Método para leer cadenas de texto
    private Token LeerCadenaTexto()
    {
        _indiceActual++; // Saltar la primera comilla
        string contenidoCadena = "";
        while (_indiceActual < _codigoFuente.Length && _codigoFuente[_indiceActual] != '"')
        {
            if (_codigoFuente[_indiceActual] == '\\' && Siguiente() == '"')
            {
                _indiceActual++; // Saltar el carácter escapado
            }
            contenidoCadena += _codigoFuente[_indiceActual];
            _indiceActual++;
        }

        if (contenidoCadena.Length > 100)
        {
            throw new Exception("Error léxico: Cadena de texto excesivamente larga.");
        }

        _indiceActual++; // Saltar la última comilla
        return new Token("CadenaTexto", contenidoCadena);
    }

    // Método para leer caracteres individuales
    private Token LeerCaracter()
    {
        _indiceActual++; // Saltar la primera comilla simple
        string contenidoCaracter = _codigoFuente[_indiceActual].ToString();
        _indiceActual++;
        if (_codigoFuente[_indiceActual] != '\'')
        {
            throw new Exception("Error léxico: Carácter no cerrado correctamente.");
        }
        _indiceActual++; // Saltar la última comilla simple
        return new Token("Caracter", contenidoCaracter);
    }

    // Método para leer comentarios de una sola línea
    private Token LeerComentarioLinea()
    {
        string comentario = "//";
        _indiceActual += 2; // Saltar "//"
        while (_indiceActual < _codigoFuente.Length && _codigoFuente[_indiceActual] != '\n')
        {
            comentario += _codigoFuente[_indiceActual];
            _indiceActual++;
        }
        return new Token("ComentarioLinea", comentario);
    }

    // Método para leer comentarios dobles (////)
    private Token LeerComentarioDoble()
    {
        string comentarioDoble = "////";
        _indiceActual += 4; // Saltar "////"
        while (_indiceActual < _codigoFuente.Length && _codigoFuente[_indiceActual] != '\n')
        {
            comentarioDoble += _codigoFuente[_indiceActual];
            _indiceActual++;
        }
        return new Token("ComentarioDoble", comentarioDoble);
    }

    // Método para leer comentarios multilínea
    private Token LeerComentarioMultilinea()
    {
        string comentarioMultilinea = "/*";
        _indiceActual += 2; // Saltar "/*"
        while (_indiceActual < _codigoFuente.Length && !(_codigoFuente[_indiceActual] == '*' && Siguiente() == '/'))
        {
            comentarioMultilinea += _codigoFuente[_indiceActual];
            _indiceActual++;
        }
        comentarioMultilinea += "*/";
        _indiceActual += 2; // Saltar "*/"
        return new Token("ComentarioMultilinea", comentarioMultilinea);
    }

    // Método para leer delimitadores y operadores
    private Token LeerDelimitadorOOperador()
    {
        char simbolo = _codigoFuente[_indiceActual];
        _indiceActual++;
        return new Token("Simbolo", simbolo.ToString());
    }

    // Método auxiliar para obtener el siguiente carácter sin avanzar
    private char Siguiente()
    {
        return _indiceActual + 1 < _codigoFuente.Length ? _codigoFuente[_indiceActual + 1] : '\0';
    }

    // Método auxiliar para "espiar" el siguiente carácter sin avanzar
    private char Peek()
    {
        return _indiceActual + 2 < _codigoFuente.Length ? _codigoFuente[_indiceActual + 2] : '\0';
    }
}

// Clase principal del programa
public class Programa
{
    public static void Main(string[] args)
    {
        try
        {
            // Crear una instancia del lexer con el archivo fuente
            Lexer lexer = new Lexer("test.txt");
            List<Token> tokens = lexer.Analizar();

            // Mostrar los tokens generados
            foreach (Token token in tokens)
            {
                Console.WriteLine(token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
