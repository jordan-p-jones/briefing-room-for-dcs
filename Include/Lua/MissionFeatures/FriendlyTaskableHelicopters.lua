briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters = {}
briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.AUTO_AIM_RADIUS = 1000 -- in meters
briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.MARKER_NAME = "helo"
briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.LANG_UNIT = "$LANG_ATTACKCHOPPERS$"
briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.LANG_REQEUEST = "$LANG_ATTACKCOPPERSREQUEST$"
briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.RADIO_REQEUEST = "RadioPilotBeginYourAttack"
briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.LANG_AFFIRM = "$LANG_BEGINATTACK$"
briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.RADIO_AFFRIM = "RadioOtherPilotBeginAttack"
briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.LANG_NO_COORDS = "$LANG_ATTACKCHOPPERSNOCOORDINATES$"
briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.markID = nil -- ID of the mark on the map
briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.disableCooRemovedRadioMessage = false


-- Radio command to launch bombing run (called from F10 menu)
function briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.launchBombingRun()
  briefingRoom.taskables.launchCurry(briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters,
    briefingRoom.mission.missionFeatures.groupNames.friendlyTaskableHelicopters, function(group, mark)
    local currPos = mist.getLeadPos(group)
    return {
      id = 'Mission',
      airborne = true,
      params = {
        route = {
          points = {
            [1] = {
              speed = 100,
              x = mark.pos.x, 0.9,
              y = mark.pos.z, 0.9,
              type = 'Turning Point',
              ETA_locked = false,
              ETA = 100,
              alt = 152.4,
              alt_type = "RADIO",
              speed_locked = false,
              action = "Fly Over Point",
              name = "CAS",
              task = {
                id = "ComboTask",
                params = {
                  tasks = {
                    [1] = {
                      enabled = true,
                      auto = false,
                      id = "EngageTargetsInZone",
                      number = 1,
                      params = {
                        y = mark.pos.z,
                        x = mark.pos.x,
                        targetTypes = {
                          [1] = "All",
                        },
                        value = "All;",
                        noTargetTypes = {
                        },
                        priority = 0,
                        zoneRadius = 5000,
                      },
                    },
                    [2] = {
                      enabled = true,
                      auto = false,
                      id = "Orbit",
                      number = 2,
                      params = {
                        altitude = 152.4,
                        pattern = "Circle",
                        speed = 100,
                      }
                    }
                  }
                }
              }
            }
          }
        },
      },
    }
  end)
end

-- Add F10 menu command
missionCommands.addCommandForCoalition(briefingRoom.playerCoalition, "$LANG_ATTACKCHOPPERMENU$",
  briefingRoom.f10Menu.missionMenu, briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters.launchBombingRun,
  nil)


-- Enable event handler
world.addEventHandler(briefingRoom.taskables.markerManager(briefingRoom.mission.missionFeatures.friendlyTaskableHelicopters))
