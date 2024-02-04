using System;
using System.Collections.Generic;

namespace BTRReportProcesser.Assets
{
    public static class CommentLookupTable
    {
        public static readonly List<String> SemiColonContainingLines = new List<String>()
        {
            "Baseline drift; ensure nose clips are on and the subject has a tight seal around mouth piece.",
            "Pneumotach reversed; ensure the picture of the person is facing the subject.",
            "Poor Inspiratory effort; have subject take deep and complete breath in prior to forceful exhalation.",
            "After deep breath instruct subject to BLAST the air out without a hesitation.Poor start of effort; late time to peak flow (FEV1)",
            "Poor start of effort; late time to peak flow (FEV1)",
            "Poor Inspiratory effort; have subject take deep and complete breath in prior to forceful exhalation.Maneuver was not started from TLC; FIVC is greater than FVC (FEV1 and FVC).",
            "Perform additional efforts; up to a maximum of eight to reach repeatability.",
            "Please have subject close the loop by taking a deep inhalation after the full exhalationManeuver was not started from TLC; FIVC is greater than FVC (FEV1 and FVC).",
            "Patient CANNOT be randomized; because data does not meet the ATS/ERS criteria for acceptable data quality at screening visit.",
            "Patient CANNOT be randomized; because FVC data does not meet the ATS/ERS criteria for acceptable data quality at screening visit.",
            "There are two linked efforts coached in all Acts; please coach only one effort in each Act",
            "Please do not perform the \"Open Circuit\" technique; use closed circuit technique. Have the subject perform relaxed tidal breathing while on the mouthpiece followed by the FVC maneuver.",
            "Please coach smaller; relaxed tidal breathing before the forced maneuver",
        };


        public static readonly Dictionary<String, String> Table = new Dictionary<string, string>()
        {
            // Sorted by value
            // also yuck

            { "", "Blank Item (Not an Error)" },

            { "After deep breath instruct subject to BLAST the air out without a hesitation.", "Start of test" },
            { "Please have the subject perform relaxed tidal breathing before the deep inspiration followed by the FVC maneuver", "Start of test" },
            { "Hesitation during start of forced exhalation back extrapolated volume is out of range (FEV1 and FVC)", "Start of test" },
            { "The tidal breaths are large compared to the FVC maneuver. Please coach smaller, relaxed tidal breathing before the FVC maneuver", "Start of test" },
            //{ "Poor Inspiratory effort; have subject take deep and complete breath in prior to forceful exhalation.", "Start of test" },
            { "Poor Inspiratory effort, have subject take deep and complete breath in prior to forceful exhalation.", "Start of test" },
            { "Hesitation during start of forced exhalation back extrapolated volume is out of range (FEV1)", "Start of test" },
            //{ "After deep breath instruct subject to BLAST the air out without a hesitation.Poor start of effort; late time to peak flow (FEV1)", "Start of Test" },
            { "After deep breath instruct subject to BLAST the air out without a hesitation.Poor start of effort, late time to peak flow (FEV1)", "Start of Test" },
            //{ "Poor start of effort; late time to peak flow (FEV1)", "Start of Test" },
            { "Poor start of effort, late time to peak flow (FEV1)", "Start of test" },

            { "Maneuver was not started from TLC, FIVC is greater than FVC (FEV1 and FVC).", "SIE" },
            //{ "Poor Inspiratory effort; have subject take deep and complete breath in prior to forceful exhalation.Maneuver was not started from TLC; FIVC is greater than FVC (FEV1 and FVC).", "SIE" },
            { "Poor Inspiratory effort, have subject take deep and complete breath in prior to forceful exhalation.Maneuver was not started from TLC, FIVC is greater than FVC (FEV1 and FVC).", "SIE" },
            //{ "Please have subject close the loop by taking a deep inhalation after the full exhalationManeuver was not started from TLC; FIVC is greater than FVC (FEV1 and FVC).", "SIE" },
            { "Please have subject close the loop by taking a deep inhalation after the full exhalationManeuver was not started from TLC, FIVC is greater than FVC (FEV1 and FVC).", "SIE" },

            { "There multiple efforts performed in Act 1. Please only perform one PFT maneuver for each act.", "Performing multiple maneuvers" },
            { "Please only perform 1 forced effort per act.  It appears there are multiple forced exhalations on each act.", "Performing multiple maneuvers" },
            { "There are multiple efforts and very large tidal breaths performed. There shouold be relaxed tidal breathing before the FVC maneuver", "Performing multiple maneuvers" },
            { "There are multiple linked efforts coached in each Act, please coach only one PFT effort in each Act", "Performing multiple maneuvers" },
            { "Looking at the RAW data there are several linked efforts coached in Acts 1 and 3-7.", "Performing multiple maneuvers" },
            { "Please only coach one PFT maneuver in each act", "Performing multiple maneuvers" },
            { "Please coach only one effort in each Act", "Performing multiple maneuvers" },


            { "Subject did not reach plateau at enexhalation, FVC may be underestimated (FVC)", "FVC may be underestimated" },
            { "Subject did not reach plateau at end exhalation, FVC may be underestimated (FVC)", "FVC may be underestimated" },

            { "Artifact/Cough during first second of effort (FEV1)", "Artifact/Cough" },
            { "Artifact/Cough during effort. Have subject rest between efforts to try and reduce cough.", "Artifact/Cough" },
            { "Artifact/Cough during effort (FVC)", "Artifact/Cough" },

            { "Poor subject effort (FEV1 and FVC)", "Poor subject effort" },
            { "Best FVC: Acceptable effort deselected by site (FVC)", "Bad Effort" },

            //{ "Patient CANNOT be randomized; because data does not meet the ATS/ERS criteria for acceptable data quality at screening visit.", "Bad Test" },
            { "Patient CANNOT be randomized, because data does not meet the ATS/ERS criteria for acceptable data quality at screening visit.", "Bad Test" },
            //{ "Patient CANNOT be randomized; because FVC data does not meet the ATS/ERS criteria for acceptable data quality at screening visit.", "Bad Test" },
            { "Patient CANNOT be randomized, because FVC data does not meet the ATS/ERS criteria for acceptable data quality at screening visit.", "Bad Test" },

            { "Coaching multiple linked efforts in a single Act is an unacceptable coaching technique. Please coach only one effort in each Act.", "Performing multiple maneuvers" },
            { "After the 2nd inspiration the subject should go back to relaxed tidal breathing. Please return to normal tidal breathing and do not perform a second forced exhalation", "Performing multiple maneuvers" },
            { "Coaching multiple linked efforts and large tidal breaths in a single Act is an unacceptable coaching technique and may exhaust the subject.  Please coach only one effort in each Act.", "Performing multiple maneuvers" },
            //{ "There are two linked efforts coached in all Acts; please coach only one effort in each Act", "Performing multiple maneuvers" },
            { "There are two linked efforts coached in all Acts, please coach only one effort in each Act", "Performing multiple maneuvers" },


            { "Poor Inspiratory effort, have subject take deep and complete breath in prior to forceful exhalation.", "SIE" },
            { "submaximal efforts FIVC significanty greater than FVC", "SIE" },

            { "FEV1 is not repeatable", "Repeatability" },
            { "FVC is not repeatable", "Repeatability" },
            { "FVC and FEV1 are not repeatable within 150cc", "Repeatability" },

            //{ "Please coach smaller; relaxed tidal breathing before the forced maneuver", "Tidal Breathing Error" },
            { "Please coach smaller, relaxed tidal breathing before the forced maneuver", "Tidal Breathing Error" },
            //{ "Please do not perform the \"Open Circuit\" technique; use closed circuit technique. Have the subject perform relaxed tidal breathing while on the mouthpiece followed by the FVC maneuver.", "Tidal Breathing Error" },
            { "Please do not perform the \"Open Circuit\" technique, use closed circuit technique. Have the subject perform relaxed tidal breathing while on the mouthpiece followed by the FVC maneuver.", "Tidal Breathing Error" },

            //{ "Perform additional efforts; up to a maximum of eight to reach repeatability.", "Less than 3 acceptable" },
            { "Perform additional efforts, up to a maximum of eight to reach repeatability.", "Less than 3 acceptable" },
            { "Perform additional efforts up to a maximum of eight to reach repeatability", "Less than 3 acceptable" },
            { "Please coach up to eight efforts to achieve acceptability and repeatability if tolerated by the subject.", "Less than 3 acceptable" },
            { "Please perform at least 3 acceptable efforts with no error codes.", "Less than 3 acceptable" },

            { "Baseline drift (FEV1 and FVC)", "Baseline drift" },
            { "Some drift during tidal breathing is noted. Please make sure subject is wearing noseclips and has a good seal on mouthpiece.", "Baseline drift" },
            { "Some significant drift during tidal breathing is noted. Please make sure subject is wearing noseclips and has a good seal on mouthpiece.", "Baseline drift" },
            { "There maybe an issue with baseline drift and zeroing of the pneumotach", "Baseline drift" },
            { "Please make sure subject is wearing noseclips and has a good seal on mouthpiece.", "Baseline drift" },

            // { "Baseline drift; ensure nose clips are on and the subject has a tight seal around mouth piece.", "Baseline drift" },
            { "Baseline drift, ensure nose clips are on and the subject has a tight seal around mouth piece.", "Baseline drift" },

            { "Please make sure the pneumotach is not reversed before testing subjects", "Pneumotach reversed " },
            { "Subject breathing on wrong side of pneumotach (FEV1 and FVC)", "Pneumotach reversed " },

            // { "Pneumotach reversed; ensure the picture of the person is facing the subject.", "Pneumotach reversed " },
            { "Pneumotach reversed, ensure the picture of the person is facing the subject.", "Pneumotach reversed " },

            { "The subject must complete the 4th phase to TLC by taking a big breath in to TLC after the forced exhalation", "Fourth Phase" },
            { "Please coach the subject to complete the 4th phase to TLC by taking a big breath in to TLC after the forced exhalation", "Fourth Phase" },
            { "Poor inspiratory effort.", "4th Phase" },

            { "Have subject continue to exhale until lungs are empty or until they can\"\"t continue any longer.", "End of test" },
            { "There is abrupt end of inhalation on all efforts. The subject is ending the inhalation after the forced exhalation too soon.", "End of test" },
            { "Please have the test subject exhale to a one second plateau", "End of test" },
            { "Abrupt end of exhalation (FEV1 and FVC)", "End of test" },
            { "Abrupt end of exhalation (FEV1)", "End of test" },
            { "Abrupt end of exhalation (FVC)", "End of test" },
            { "This is an unacceptable coaching technique and should be discontinued", "End of test" },
            { "Please have subject close the loop by taking a deep inhalation after the full exhalation", "End of test" },
            { "This is an unacceptable coaching technique and should be stopped.", "End of test" },

            // Misc
            { "Please do not coach an ERV maneuver prior to the deep inhalation to TLC before the forced exhalation", "Exhaling to ERV" },
            { "Please do not have the subject perform the \"Open Circuit\" technique. Have the subject perform relaxed tidal breathing before the FVC maneuver.", "Open Circuit " },
            { "Please perform acts from same subject", "Possible Mix up of subjects" }
        };
    }
}
