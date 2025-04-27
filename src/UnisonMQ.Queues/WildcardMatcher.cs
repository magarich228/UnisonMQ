namespace UnisonMQ.Queues;

public static class WildcardMatcher
{
    public static bool IsMatch(string subject, string pattern)
    {
        // Разделяем тему и шаблон на части по точкам
        string[] subjectTokens = subject.Split('.');
        string[] patternTokens = pattern.Split('.');

        int sIdx = 0, pIdx = 0;

        while (pIdx < patternTokens.Length && sIdx < subjectTokens.Length)
        {
            string patternPart = patternTokens[pIdx];

            // Обработка wildcard '>'
            if (patternPart == ">")
            {
                // '>' должен быть последней частью в шаблоне
                if (pIdx != patternTokens.Length - 1)
                    return false;

                return true; // Соответствует всем оставшимся частям
            }

            // Обработка wildcard '*'
            if (patternPart == "*")
            {
                pIdx++;
                sIdx++;
                continue;
            }

            // Обычное сравнение
            if (patternPart != subjectTokens[sIdx])
                return false;

            pIdx++;
            sIdx++;
        }

        // Проверяем остатки после обработки
        if (pIdx == patternTokens.Length && sIdx == subjectTokens.Length)
            return true;

        // Обработка случая когда шаблон короче темы, но есть wildcard '>' в конце
        if (pIdx > 0 && patternTokens[pIdx - 1] == ">")
            return true;

        // Обработка случая когда тема закончилась, но остался wildcard '*' или '>'
        while (pIdx < patternTokens.Length)
        {
            if (patternTokens[pIdx] == ">")
            {
                if (pIdx != patternTokens.Length - 1)
                    return false;
                return true;
            }

            if (patternTokens[pIdx] != "*")
                return false;
            pIdx++;
        }

        return sIdx == subjectTokens.Length;
    }
}