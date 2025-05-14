using Rimu.Repository.Postgres.Adapter.Interface;

namespace Rimu.Repository.Postgres.Adapter.Dto;

public sealed class UserStatsDto: IUserStatsReadonly {
    public long UserId { get; private init;  }
    public long OverallPlaycount { get; private init;  }
    public long OverallScore { get; private init;  }
    public double OverallAccuracy { get; private init;  }
    public double OverallPp { get; private init;  }
    public long OverallCombo { get; private init;  }
    public long OverallXss { get; private init;  }
    public long OverallSs { get; private init;  }
    public long OverallXs { get; private init;  }
    public long OverallS { get; private init;  }
    public long OverallA { get; private init;  }
    public long OverallB { get; private init;  }
    public long OverallC { get; private init;  }
    public long OverallD { get; private init;  }
    public long OverallPerfect { get; private init;  }
    public long OverallHits { get; private init;  }
    public long Overall300 { get; private init;  }
    public long Overall100 { get; private init;  }
    public long Overall50 { get; private init;  }
    public long OverallGeki { get; private init;  }
    public long OverallKatu { get; private init;  }
    public long OverallMiss { get; private init;  }

    public UserStatsDto(long userId, long overallPlaycount, long overallScore, double overallAccuracy, double overallPp, long overallCombo, long overallXss, long overallSs, long overallXs, long overallS, long overallA, long overallB, long overallC, long overallD, long overallPerfect, long overallHits, long overall300, long overall100, long overall50, long overallGeki, long overallKatu, long overallMiss) {
        UserId = userId;
        OverallPlaycount = overallPlaycount;
        OverallScore = overallScore;
        OverallAccuracy = overallAccuracy;
        OverallPp = overallPp;
        OverallCombo = overallCombo;
        OverallXss = overallXss;
        OverallSs = overallSs;
        OverallXs = overallXs;
        OverallS = overallS;
        OverallA = overallA;
        OverallB = overallB;
        OverallC = overallC;
        OverallD = overallD;
        OverallPerfect = overallPerfect;
        OverallHits = overallHits;
        Overall300 = overall300;
        Overall100 = overall100;
        Overall50 = overall50;
        OverallGeki = overallGeki;
        OverallKatu = overallKatu;
        OverallMiss = overallMiss;
    }

    public static UserStatsDto From(IUserStatsReadonly userStats) {
        return new UserStatsDto(
            userId: userStats.UserId,
            overallPlaycount: userStats.OverallPlaycount,
            overallScore: userStats.OverallScore,
            overallAccuracy: userStats.OverallAccuracy,
            overallPp: userStats.OverallPp,
            overallCombo: userStats.OverallCombo,
            overallXss: userStats.OverallXss,
            overallSs: userStats.OverallSs,
            overallXs: userStats.OverallXs,
            overallS: userStats.OverallS,
            overallA: userStats.OverallA,
            overallB: userStats.OverallB,
            overallC: userStats.OverallC,
            overallD: userStats.OverallD,
            overallPerfect: userStats.OverallPerfect,
            overallHits: userStats.OverallHits,
            overall300: userStats.Overall300,
            overall100: userStats.Overall100,
            overall50: userStats.Overall50,
            overallGeki: userStats.OverallGeki,
            overallKatu: userStats.OverallKatu,
            overallMiss: userStats.OverallMiss    
        );
    }
}