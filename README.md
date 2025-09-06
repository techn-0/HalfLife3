# HalfLife3
스마일게이트 Stove Infinity 2025 HalfLife3 팀

## 📖 프로젝트 개요
HalfLife3는 Unity 기반의 룸 데코레이션 시뮬레이션 게임입니다. 플레이어는 가상의 방을 꾸미고, 일일 퀘스트를 수행하며, 다양한 아이템을 구매할 수 있는 인터랙티브한 경험을 제공합니다.

## 🎮 주요 기능

### 🏠 룸 데코레이션 시스템
- 방 내 오브젝트들의 동적 배치 및 관리
- PlayerPrefs를 통한 방 상태 저장 및 로드
- 실시간 룸 상태 업데이트

### 🛒 상점 시스템
- 코인 기반 경제 시스템
- 카테고리별 아이템 분류 (메인/서브 카테고리)
- 탭 기반 상점 UI 인터페이스

### 📋 일일 퀘스트 시스템
- 데일리 퀘스트 관리 및 진행도 추적
- 퀘스트 완료 보상 지급

### 🎁 출석 보상 시스템
- 일일 출석 체크 및 보상 지급
- 출석 팝업 UI 관리

## 🏗️ 프로젝트 구조

```
Assets/
├── 01_Scene/           # Unity 씬 파일들
│   ├── daily questUI.unity
│   └── UIScene.unity
├── 02_Scripts/         # C# 스크립트 파일들
│   ├── Common/         # 공통 유틸리티
│   ├── DailyQuests/    # 일일 퀘스트 관련
│   ├── ETC/           # 기타 스크립트
│   ├── Http/          # HTTP 통신 관련
│   ├── Knowledge/     # 지식/정보 관리
│   ├── Managers/      # 게임 매니저들
│   ├── Object/        # 게임 오브젝트
│   ├── Reward/        # 보상 시스템
│   ├── Shop/          # 상점 시스템
│   └── UI/            # UI 관련 스크립트
├── 03_Resource/       # 게임 리소스
│   ├── ETC/
│   ├── human/         # 캐릭터 리소스
│   ├── Room/          # 룸 관련 리소스
│   └── UI/            # UI 리소스
├── 04_Prefabs/        # Unity 프리팹들
├── 05_Animation/      # 애니메이션 파일들
└── 06_Font/          # 폰트 파일들
```

## 🔧 핵심 컴포넌트

### 매니저 시스템
- **GameManager**: 게임의 전반적인 상태 관리 (싱글톤 패턴)
- **CoinManager**: 게임 내 코인 시스템 관리
- **EffectManager**: 이펙트 관리
- **EventManager**: 이벤트 시스템 관리
- **AttendanceRewardInitializer**: 출석 보상 초기화

### UI 시스템
- **AttendancePopupUI**: 출석 보상 팝업
- **DecoPopUp**: 데코레이션 팝업
- **ShopElement/ShopTab**: 상점 UI 요소들
- **MainCategoryController/SubCategoryController**: 카테고리 관리
- **Overlay**: UI 오버레이 관리

### 룸 시스템
- **RoomController**: 룸 상태 및 오브젝트 관리
- **RoomObjectBase**: 룸 내 오브젝트 베이스 클래스

## 🛠️ 기술 스택
- **Engine**: Unity 2022.3+ (URP)
- **Language**: C#
- **UI Framework**: Unity UI System
- **Input System**: Unity Input System
- **Package Dependencies**:
  - AYellowpaper.SerializedCollections
  - Simple Inspector Attributes
  - Simple Scroll-Snap
  - TextMesh Pro

## 🚀 개발 환경 설정

### 필수 요구사항
- Unity 2022.3 LTS 이상
- Visual Studio 2022 또는 JetBrains Rider
- .NET Framework 4.7.1+

### 프로젝트 실행
1. Unity Hub에서 프로젝트 열기
2. 필요한 패키지들이 자동으로 설치되는지 확인
3. `01_Scene/UIScene.unity` 씬에서 게임 실행

## 📋 게임플레이 흐름
1. **게임 시작**: 기본 룸 상태 로드
2. **룸 탐색**: 방 내 오브젝트들과 상호작용
3. **상점 이용**: 코인으로 새로운 아이템 구매
4. **퀘스트 수행**: 일일 퀘스트 완료로 보상 획득
5. **출석 체크**: 매일 로그인 보상 수령
6. **룸 꾸미기**: 구매한 아이템으로 방 데코레이션

## 🎯 주요 특징
- **지속성**: PlayerPrefs를 통한 게임 상태 저장
- **모듈화**: 컴포넌트 기반 아키텍처
- **확장성**: 새로운 아이템 및 기능 추가 용이
- **사용자 친화적**: 직관적인 UI/UX 디자인

## 👥 개발팀
스마일게이트 Stove Infinity 2025 HalfLife3 팀

