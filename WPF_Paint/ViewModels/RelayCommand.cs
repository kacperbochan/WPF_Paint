using System;
using System.Windows.Input;

public class RelayCommand : ICommand
{
    // Pole przechowujące logikę wykonywania komendy
    private readonly Action<object> _execute;

    // Pole przechowujące logikę sprawdzania, czy komenda może zostać wykonana
    private readonly Predicate<object> _canExecute;

    // Konstruktor przyjmujący akcję do wykonania
    public RelayCommand(Action<object> execute)
        : this(execute, null)
    {
    }

    // Główny konstruktor przyjmujący akcję do wykonania oraz warunek sprawdzający możliwość wykonania komendy
    public RelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
        // Jeśli nie podano akcji, zgłoś błąd
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        // Przypisz warunek sprawdzający
        _canExecute = canExecute;
    }

    // Metoda sprawdzająca, czy komenda może zostać wykonana
    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    // Zdarzenie informujące o zmianie możliwości wykonania komendy
    // Wykorzystuje mechanizm CommandManager do automatycznego sprawdzania stanu komendy
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    // Metoda wykonująca logikę komendy
    public void Execute(object parameter)
    {
        _execute(parameter);
    }
}
