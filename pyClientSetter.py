import grpc
import cvTransfer_pb2
import cvTransfer_pb2_grpc
import cv2
import mediapipe as mp

mp_drawing = mp.solutions.drawing_utils
mp_drawing_styles = mp.solutions.drawing_styles
mp_holistic = mp.solutions.holistic
mp_hands = mp.solutions.hands

channel = grpc.insecure_channel('localhost:50051')
stub = cvTransfer_pb2_grpc.PointSenderStub(channel)
setOfPoints = cvTransfer_pb2.AllPoints()

#create hand points
for i in range(21):
    setOfPoints.vec.add()
    setOfPoints.vec[i].x = 1.0
    setOfPoints.vec[i].y = 1.0
    setOfPoints.vec[i].z = 1.0

# For webcam input:
cap = cv2.VideoCapture(0)
with mp_holistic.Holistic(min_detection_confidence=0.7, min_tracking_confidence=0.5) as holistic, mp_hands.Hands(max_num_hands=1, min_detection_confidence=0.7, min_tracking_confidence=0.5) as hands:
  while cap.isOpened():
    success, image = cap.read()
    if not success:
      print("Ignoring empty camera frame.")
      # If loading a video, use 'break' instead of 'continue'.
      continue

    # Flip the image horizontally for a later selfie-view display, and convert
    # the BGR image to RGB.
    image = cv2.cvtColor(cv2.flip(image, 1), cv2.COLOR_BGR2RGB)
    # To improve performance, optionally mark the image as not writeable to
    # pass by reference.
    image.flags.writeable = False
    results = holistic.process(image)
    resultsH = hands.process(image)

    # Draw landmark annotation on the image.
    image.flags.writeable = True
    image = cv2.cvtColor(image, cv2.COLOR_RGB2BGR)
    mp_drawing.draw_landmarks(
        image,
        results.face_landmarks,
        mp_holistic.FACEMESH_CONTOURS,
        landmark_drawing_spec=None,
        connection_drawing_spec=mp_drawing_styles
        .get_default_face_mesh_contours_style())
    mp_drawing.draw_landmarks(
        image,
        results.pose_landmarks,
        mp_holistic.POSE_CONNECTIONS,
        landmark_drawing_spec=mp_drawing_styles
        .get_default_pose_landmarks_style())
    if resultsH.multi_hand_landmarks:
      for hand_landmarks in resultsH.multi_hand_landmarks:
        for id, lm in enumerate(hand_landmarks.landmark):
                print(id, lm)
                # Get finger joint points
                h, w, c = image.shape
                cx, cy = int(lm.x*w), int(lm.y*h)
                cv2.putText(image, str(int(id)), (cx+10, cy+10), cv2.FONT_HERSHEY_PLAIN,
                            1, (0, 0, 255), 2)
                setOfPoints.vec[id].x = lm.x
                setOfPoints.vec[id].y = lm.y
                setOfPoints.vec[id].z = lm.z
        mp_drawing.draw_landmarks(
            image,
            hand_landmarks,
            mp_hands.HAND_CONNECTIONS,
            mp_drawing_styles.get_default_hand_landmarks_style(),
            mp_drawing_styles.get_default_hand_connections_style())
    cv2.imshow('MediaPipe Holistic', image)

    #send setOfPoints to server
    response = stub.SetPoints(setOfPoints)

    if cv2.waitKey(1) == ord('q'):
      break
cap.release()
cv2.destroyAllWindows()

print("done");
