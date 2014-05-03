package networking.response;

/**
 *
 * @author Aleck
 */

// Java Imports
import java.util.ArrayList;
import java.util.List;
import model.TutorialData;
import metadata.Constants;
import utility.GamePacket;

public class ResponseTutorial extends GameResponse
{
    private ArrayList<TutorialData> tutorialList;
    public ResponseTutorial() 
    {
        responseCode = Constants.SMSG_TUTORIAL_DATA;
    }
    
    @Override
    public byte[] constructResponseInBytes() {
        GamePacket packet = new GamePacket(responseCode);
        
        packet.addShort16((short) tutorialList.size());

        for (TutorialData tutorial : tutorialList) {
            packet.addInt32(tutorial.getPlayerID());
            packet.addInt32(tutorial.getMilestone());
            packet.addInt32(tutorial.getCurTut());
            packet.addInt32(tutorial.getCurChallenge());
        }

        return packet.getBytes();
    }
    
    public void updateTutorial(ArrayList<TutorialData> tutorialList)
    {
        this.tutorialList = tutorialList;
        
    }
}
