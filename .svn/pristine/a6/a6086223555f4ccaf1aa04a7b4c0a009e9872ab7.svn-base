package networking.request;

/**
 *
 * @author Aleck
 
 */
import java.io.IOException;

// Java Imports
import dataAccessLayer.TutorialDAO;
import dataAccessLayer.PlayerDAO;
import java.io.IOException;
import utility.DataReader;
import model.Avatar;
import model.TutorialData;
import networking.response.ResponseTutorial;

public class RequestTutorial extends GameRequest
{
    private int p_id;
    private int cur_tut;
    private int milestone;
    private String message;

    private ResponseTutorial responseTutorial;
    
    public RequestTutorial() 
    {
        responses.add(responseTutorial = new ResponseTutorial());
    }
    
    @Override
    public void parse() throws IOException 
    {
    }
    
    @Override
    public void doBusiness() throws Exception 
    {
        p_id = client.getPlayer().getID();
        TutorialData tutorial = new TutorialData(p_id,cur_tut,milestone,message);
        TutorialDAO.initializeTutorial(p_id);
        ResponseTutorial responseTutorial = new ResponseTutorial();
        responseTutorial.updateTutorial(TutorialDAO.updateTutData(tutorial));
        responses.add(responseTutorial);
    }

    
}
