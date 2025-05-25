namespace AiChatFrontend.Helpers;

public static class LlmModelHelper
{
    public static string GetDefaulModelId(List<LlmModel> models, LlmModelType type)
    {
        var model = models.First(x => x.ModelType == type);
        return model.DefaultModelId;
    }

    public static IEnumerable<string> GetModelIds(List<LlmModel> models, LlmModelType type)
    {
        var model = models.First(x => x.ModelType == type);
        return model.ModelIds;
    }

    public static IEnumerable<string> GetModelIds(LlmModel model)
    {
        return model.ModelIds;
    }
}
