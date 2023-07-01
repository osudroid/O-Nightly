using System.Runtime.CompilerServices;

namespace OsuDroid.Lib.DbTransfer; 

public class DbTransfer {
    private bool _cleanDb = false;
    private bool _insertUser = false;
    private bool _insertScore = false;
    private bool _calcUserScore = false;

    public bool CleanDb => _cleanDb;

    public bool InsertUser => _insertUser;

    public bool InsertScore => _insertScore;

    public bool CalcUserScore => _calcUserScore;

    public DbTransfer UseCleanDb() {
        _cleanDb = true;
        return this;
    }

    public DbTransfer UseInsertUser() {
        _insertUser = true;
        return this;
    }

    public DbTransfer UseInsertScore() {
        _insertScore = true;
        return this;
    }

    public DbTransfer UseCalcUserScore() {
        _calcUserScore = true;
        return this;
    }

    public async Task Run() {
        try {
            // if (_cleanDb) await CleanDbHandler.Run(); 
            // if (_insertUser) await InsertUserHandler.Run(); 
            // if (_insertScore) await InsertScoreHandler.Run(); 
            if (_calcUserScore) await CalcUserScoreHandler.Run(); 
        }
        catch (Exception e) {
            WriteLine(e);
            System.Environment.Exit(1);
        }
    }
}