namespace OsuDroid.Lib.DbTransfer;

public class DbTransfer {
    public bool CleanDb { get; private set; }

    public bool InsertUser { get; private set; }

    public bool InsertScore { get; private set; }

    public bool CalcUserScore { get; private set; }

    public DbTransfer UseCleanDb() {
        CleanDb = true;
        return this;
    }

    public DbTransfer UseInsertUser() {
        InsertUser = true;
        return this;
    }

    public DbTransfer UseInsertScore() {
        InsertScore = true;
        return this;
    }

    public DbTransfer UseCalcUserScore() {
        CalcUserScore = true;
        return this;
    }

    public async Task Run() {
        try {
            // if (_cleanDb) await CleanDbHandler.Run(); 
            // if (_insertUser) await InsertUserHandler.Run(); 
            // if (_insertScore) await InsertScoreHandler.Run(); 
            if (CalcUserScore) await CalcUserScoreHandler.Run();
        }
        catch (Exception e) {
            WriteLine(e);
            Environment.Exit(1);
        }
    }
}